# example file from https://github.com/Daniil-Osokin/lightweight-human-pose-estimation-3d-demo.pytorch
# modified by Jonathan Lehner
# added websockets and connected it to Unity
# This file has to be moved inside the lightweight-human-pose-estimation-3d-demo.pytorch folder.
# Installation on windows is a bit tricky. I used python 3.7 and 
# pip install torch===1.4.0 torchvision===0.5.0 -f https://download.pytorch.org/whl/torch_stable.html
# On windows I had to add time.sleep(2) to the VideoReader class, in the line before
# self.cap = cv2.VideoCapture(0+cv2.CAP_DSHOW) (in input_reader.py)
# Run this file with
# cd C:\poseteacher2\lightweight-human-pose-estimation-3d-demo.pytorch
# pipenv shell (recommended to use a Virtual Environment)
# python demo_ws.py --model human-pose-estimation-3d.pth --video 0
# The lightweight-human-pose-estimation-3d-demo.pytorch repo uses a common keypoint format.
# This is from the PanopticStudio Toolbox (https://github.com/CMU-Perceptual-Computing-Lab/panoptic-toolbox).
'''
0: Neck
1: Nose
2: BodyCenter (center of hips)
3: lShoulder
4: lElbow
5: lWrist,
6: lHip
7: lKnee
8: lAnkle
9: rShoulder
10: rElbow
11: rWrist
12: rHip
13: rKnee
14: rAnkle
15: rEye
16: lEye
17: rEar
18: lEar
'''

from argparse import ArgumentParser
import json
import os
import cv2
import numpy as np
from modules.input_reader import VideoReader, ImageReader
from modules.draw import Plotter3d, draw_poses
from modules.parse_poses import parse_poses
import asyncio
import websockets
import json

# https://stackoverflow.com/questions/26646362/numpy-array-is-not-json-serializable
class NumpyEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, np.ndarray):
            return obj.tolist()
        return json.JSONEncoder.default(self, obj)

def rotate_poses(poses_3d, R, t):
    R_inv = np.linalg.inv(R)
    for pose_id in range(len(poses_3d)):
        pose_3d = poses_3d[pose_id].reshape((-1, 4)).transpose()
        pose_3d[0:3, :] = np.dot(R_inv, pose_3d[0:3, :] - t)
        poses_3d[pose_id] = pose_3d.transpose().reshape(-1)

    return poses_3d

async def hello(websocket, path):
    name = await websocket.recv()
    print(f"< {name}")

    greeting = f"Hello {name}!"

    await websocket.send(greeting)
    print(f"> {greeting}")

frame_provider = None
fx = 0

if __name__ == '__main__':
    parser = ArgumentParser(description='Lightweight 3D human pose estimation demo. '
                                        'Press esc to exit, "p" to (un)pause video or process next image.')
    parser.add_argument('-m', '--model',
                        help='Required. Path to checkpoint with a trained model '
                             '(or an .xml file in case of OpenVINO inference).',
                        type=str, required=True)
    parser.add_argument('--video', help='Optional. Path to video file or camera id.', type=str, default='')
    parser.add_argument('-d', '--device',
                        help='Optional. Specify the target device to infer on: CPU or GPU. '
                             'The demo will look for a suitable plugin for device specified '
                             '(by default, it is GPU).',
                        type=str, default='GPU')
    parser.add_argument('--use-openvino',
                        help='Optional. Run network with OpenVINO as inference engine. '
                             'CPU, GPU, FPGA, HDDL or MYRIAD devices are supported.',
                        action='store_true')
    parser.add_argument('--images', help='Optional. Path to input image(s).', nargs='+', default='')
    parser.add_argument('--height-size', help='Optional. Network input layer height size.', type=int, default=256)
    parser.add_argument('--extrinsics-path',
                        help='Optional. Path to file with camera extrinsics.',
                        type=str, default=None)
    parser.add_argument('--fx', type=np.float32, default=-1, help='Optional. Camera focal length.')
    args = parser.parse_args()

    if args.video == '' and args.images == '':
        raise ValueError('Either --video or --image has to be provided')

    stride = 8
    if args.use_openvino:
        from modules.inference_engine_openvino import InferenceEngineOpenVINO
        net = InferenceEngineOpenVINO(args.model, args.device)
    else:
        from modules.inference_engine_pytorch import InferenceEnginePyTorch
        net = InferenceEnginePyTorch(args.model, args.device)

    canvas_3d = np.zeros((720, 1280, 3), dtype=np.uint8)
    plotter = Plotter3d(canvas_3d.shape[:2])
    canvas_3d_window_name = 'Canvas 3D'
    cv2.namedWindow(canvas_3d_window_name)
    cv2.setMouseCallback(canvas_3d_window_name, Plotter3d.mouse_callback)

    file_path = args.extrinsics_path
    if file_path is None:
        file_path = os.path.join('data', 'extrinsics.json')
    with open(file_path, 'r') as f:
        extrinsics = json.load(f)
    R = np.array(extrinsics['R'], dtype=np.float32)
    t = np.array(extrinsics['t'], dtype=np.float32)

    frame_provider = ImageReader(args.images)
    is_video = False
    if args.video != '':
        frame_provider = VideoReader(args.video)
        is_video = True
    base_height = args.height_size
    fx = args.fx

    async def estimate(websocket, path):
        name = await websocket.recv()
        print(f"< {name}")
        global fx
        delay = 1
        esc_code = 27
        p_code = 112
        space_code = 32
        mean_time = 0

        for frame in frame_provider:
            current_time = cv2.getTickCount()
            if frame is None:
                break
            input_scale = base_height / frame.shape[0]
            scaled_img = cv2.resize(frame, dsize=None, fx=input_scale, fy=input_scale)
            scaled_img = scaled_img[:, 0:scaled_img.shape[1] - (scaled_img.shape[1] % stride)]  # better to pad, but cut out for demo
            if fx < 0:  # Focal length is unknown
                fx = np.float32(0.8 * frame.shape[1])

            inference_result = net.infer(scaled_img)
            poses_3d, poses_2d = parse_poses(inference_result, input_scale, stride, fx, is_video)
            edges = []
            if len(poses_3d):
                poses_3d = rotate_poses(poses_3d, R, t)
                poses_3d_copy = poses_3d.copy()
                x = poses_3d_copy[:, 0::4]
                y = poses_3d_copy[:, 1::4]
                z = poses_3d_copy[:, 2::4]
                poses_3d[:, 0::4], poses_3d[:, 1::4], poses_3d[:, 2::4] = -z, x, -y

                poses_3d = poses_3d.reshape(poses_3d.shape[0], 19, -1)[:, :, 0:3]
                edges = (Plotter3d.SKELETON_EDGES + 19 * np.arange(poses_3d.shape[0]).reshape((-1, 1, 1))).reshape((-1, 2))
            plotter.plot(canvas_3d, poses_3d, edges)
            cv2.imshow(canvas_3d_window_name, canvas_3d)

            draw_poses(frame, poses_2d)

            current_time = (cv2.getTickCount() - current_time) / cv2.getTickFrequency()
            if mean_time == 0:
                mean_time = current_time
            else:
                mean_time = mean_time * 0.95 + current_time * 0.05
            cv2.putText(frame, 'FPS: {}'.format(int(1 / mean_time * 10) / 10),
                        (40, 80), cv2.FONT_HERSHEY_COMPLEX, 1, (0, 0, 255))
            cv2.imshow('ICV 3D Human Pose Estimation', frame)

            greeting = f"{json.dumps(poses_3d, cls=NumpyEncoder)}"
            await websocket.send(greeting)
            print(f"> {greeting}")

            key = cv2.waitKey(delay)
            if key == esc_code:
                break
            if key == p_code:
                if delay == 1:
                    delay = 0
                else:
                    delay = 1
            if delay == 0 or not is_video:  # allow to rotate 3D canvas while on pause
                key = 0
                while (key != p_code
                    and key != esc_code
                    and key != space_code):
                    plotter.plot(canvas_3d, poses_3d, edges)
                    cv2.imshow(canvas_3d_window_name, canvas_3d)
                    key = cv2.waitKey(33)
                if key == esc_code:
                    break
                else:
                    delay = 1


    start_server = websockets.serve(estimate, "localhost", 2567)
    asyncio.get_event_loop().run_until_complete(start_server)
    asyncio.get_event_loop().run_forever()