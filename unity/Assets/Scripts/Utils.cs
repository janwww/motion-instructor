using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Globalization;

namespace PoseTeacher
{
	class Utils //: MonoBehaviour
	{
		public string file_joints;
		public List<List<int>> weights_joints;
        public Dictionary<string, List<double>> weights_dict;
        double weight;
        int index;
        public Utils() { }
		public void set_weights(string file)
        {
            using (var rd = new StreamReader(file))
            {
                var names = rd.ReadLine().Split(',');
                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(',');
                    index = 0;
                    for (int index = 0; index < splits.Count(); index++)
                    {
                        weight = Double.Parse(splits[index]);
                        //weights_dict[names[index]].Add(weights_dict);
                        //weights_joints[index].Add(weight);
                        if (weights_dict.ContainsKey(names[index]))
                        {
                            weights_dict[names[index]].Add(weight);
                        }
                        else
                        {
                            weights_dict.Add(names[index], new List<double> { weight });
                        }
                    }
                    //foreach (var (item, index) in splits.LoopIndex())
                    //    foreach var split in splits{
                    //    weights_joints[index].Add(split);

                    //}
                }
            }
        }
	}
}