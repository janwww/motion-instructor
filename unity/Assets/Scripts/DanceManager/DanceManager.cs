using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PoseTeacher {
    public class DanceManager : MonoBehaviour {
        public static DanceManager instance;
        public static float SongTime => instance.audioSource.time;
        public static DanceData DanceData => instance.danceData;

        public DancePerformanceScriptableObject[] tracks;
        public int startingTrack = 0;
        public bool playOnStart = false;

        // would love to make this into a prefab but it does not work
        //public GameObject GoalDisplayObject;

        public GameObject AvatarDisplayPrefab;
        public GameObject dancePoseSourcePrefab;

        public ScoreDisplay scoreDisplay;

        private int currentTrack = 0;
        private DanceData danceData;
        private List<IDanceGoal> goals;
        private bool isRunning = false;
        private bool isReady = false;
        private AudioSource audioSource;
        private float score = 0;
        private float poseGoalTestTime = 1f;
        private int currentGoal = 0;
        private int currentPose = 0;
        private List<float> runningScores = new List<float>();

        private IAvatarDisplay avatarDisplay;
        //private IGoalDisplay goalDisplay;
        private IScoringFunction scoringFunction = new ScoringQuaternionDistance();
        private IDancePoseSource dancePoseSource;


        private void Awake() {
            // make singleton
            if (instance == null) {
                DontDestroyOnLoad(gameObject);
                instance = this;
                currentTrack = startingTrack;
                audioSource = GetComponent<AudioSource>();
                //goalDisplay = GoalDisplayObject.GetComponent<IGoalDisplay>();

                if (playOnStart) {
                    Invoke("Play", 0.5f);
                }
            } else {
                Destroy(gameObject);
            }
        }

        private void init() {
            score = 0;
            danceData = tracks[currentTrack].danceData.LoadDanceDataFromScriptableObject();
            goals = tracks[currentTrack].goals;
            audioSource.clip = tracks[currentTrack].SongObject.SongClip;
            audioSource.time = 0f;
            currentGoal = 0;
            currentPose = 0;
            runningScores = new List<float>();

            // Restore the gameobjects
            if (avatarDisplay != null) {
                Destroy(avatarDisplay.gameObject);
            }
            avatarDisplay = Instantiate(AvatarDisplayPrefab).GetComponent<IAvatarDisplay>();

            //goalDisplay.showNothing();

            if (dancePoseSource != null) {
                Destroy(dancePoseSource.gameObject);
            }
            // todo maybe need to do this differently for kinect source, needs time to find player
            dancePoseSource = Instantiate(dancePoseSourcePrefab).GetComponent<IDancePoseSource>();

            isReady = true;
            Debug.Log("Init done");
        }

        public void SwitchTrack(int newTrack) {
            currentTrack = newTrack;
            isReady = false;
            Debug.Log("Switching Tracks");
            init();
        }

        public void Pause() {
            if (!isRunning) {
                return;
            }

            Debug.Log("Pause");
            isRunning = false;
            audioSource.Pause();
        }

        public void Play() {
            if (isRunning) {
                return;
            }

            // if not ready yet, init and call in .5 seconds again
            if (!isReady) {
                init();
                Invoke("Play", 0.5f);
            }
            Debug.Log("Play");
            isRunning = true;
            audioSource.Play();
        }

        void Update() {
            // Check if song has ended
            if (!audioSource.isPlaying && isRunning) {
                isReady = false;
                isRunning = false;
                //goalDisplay.showNothing();
                Destroy(avatarDisplay.gameObject);

                // Todo display finish screen
            }

            if (isRunning) {
                // Update avatar with interpolated DancePose
                float timeOffset = audioSource.time - danceData.poses[currentPose].timestamp;
                DancePose interpolatedPose = danceData.GetInterpolatedPose(currentPose, out currentPose, timeOffset);
                avatarDisplay.SetPose(interpolatedPose);

                // Todo outsource this to a scoring manager
                // check if goal is over and calculate score
                while (currentGoal < goals.Count) {
                    IDanceGoal goal = goals[currentGoal];
                    if (goal.GetGoalType() == GoalType.POSE) {
                        DanceGoalPose goalPose = (DanceGoalPose)goal;
                        if (SongTime > danceData.poses[goalPose.id].timestamp + poseGoalTestTime) {
                            updateScore();
                            currentGoal++;
                            Debug.Log("new goal: " + currentGoal);
                        } else {
                            break; // the loop
                        }
                    } else if (goal.GetGoalType() == GoalType.MOTION) {
                        DanceGoalMotion goalMotion = (DanceGoalMotion)goal;
                        if (SongTime > danceData.poses[goalMotion.endId].timestamp) {
                            updateScore();
                            currentGoal++;
                        } else {
                            break; // the loop
                        }
                    }
                }

                // Display Goal and maybe do a scoring run
                if (currentGoal < goals.Count) {
                    // Update GoalDisplay
                    //goalDisplay.showGoal(danceData, goals[currentGoal], SongTime);

                    // Calculate Score (Todo maybe do this on a timer for framerate independance)
                    IDanceGoal goal = goals[currentGoal];
                    if (goal.GetGoalType() == GoalType.POSE) {
                        DanceGoalPose goalPose = (DanceGoalPose)goal;
                        if (SongTime > danceData.poses[goalPose.id].timestamp - poseGoalTestTime) {
                            runningScores.Add(scoringFunction.GetScore(danceData.poses[goalPose.id], dancePoseSource.GetDancePose()));
                        }
                    } else if (goal.GetGoalType() == GoalType.MOTION) {
                        DanceGoalMotion goalMotion = (DanceGoalMotion)goal;
                        if (SongTime > danceData.poses[goalMotion.startId].timestamp) {
                            runningScores.Add(scoringFunction.GetScore(interpolatedPose, dancePoseSource.GetDancePose()));
                        }
                    }
                } else {
                    //goalDisplay.showNothing();
                }
            }
        }

        // goal has ended, add scores and display TM
        private void updateScore() {
            float scoreToAdd = 0;
            IDanceGoal goal = goals[currentGoal];
            if (goal.GetGoalType() == GoalType.POSE) {
                if (runningScores.Count > 0) {
                    scoreToAdd = runningScores.Max();
                }
            } else if (goal.GetGoalType() == GoalType.MOTION) {
                if (runningScores.Count > 0) {
                    scoreToAdd = runningScores.Max();
                }
            }

            if (runningScores.Count == 0) {
                Debug.Log("Is this correct?");
            }

            runningScores = new List<float>();
            displayScore(scoreToAdd);
            score += scoreToAdd;
            Debug.Log("new score: " + score);
        }

        private void displayScore(float score) {
            if (score > 0.85) {
                scoreDisplay.addScore(Scores.GREAT);
            } else if (score > 0.6) {
                scoreDisplay.addScore(Scores.GOOD);
            } else {
                scoreDisplay.addScore(Scores.BAD);
            }
        }
    }
}
