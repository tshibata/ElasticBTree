/*
 * Copyright(c) 2013-2014 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using ElasticBTree;

namespace Benchmark
{
	class Benchmark
	{
		int size;
		
		int parallelism;
		
		ElasticBTree<int, string> tree = new ElasticBTree<int, string> ();
		
		double insertTime, findTime, deleteTime, totalTime;
		
		Benchmark (int size, int parallelism)
		{
			this.size = size;
			this.parallelism = parallelism;
		}
		
		public static void Main (string[] args)
		{
			Benchmark[] benchmark = new Benchmark [8];
			for (int i = 0; i < benchmark.Length; i ++) {
				benchmark[i] = new Benchmark (1000 * 1000, i + 1);
				benchmark[i].Run ();
			}
			using (StreamWriter w = new StreamWriter(new FileStream("insert.dat", FileMode.Create))) {
				for (int i = 0; i < benchmark.Length; i ++) {
					w.WriteLine((i + 1) + " " + benchmark[i].insertTime);
				}
			}
			using (StreamWriter w = new StreamWriter(new FileStream("find.dat", FileMode.Create))) {
				for (int i = 0; i < benchmark.Length; i ++) {
					w.WriteLine((i + 1) + " " + benchmark[i].findTime);
				}
			}
			using (StreamWriter w = new StreamWriter(new FileStream("delete.dat", FileMode.Create))) {
				for (int i = 0; i < benchmark.Length; i ++) {
					w.WriteLine((i + 1) + " " + benchmark[i].deleteTime);
				}
			}
		}
		
		static int Pseudorandom (int prev)
		{
			return (prev * 1103515245 + 12345) & 0x7FFFFFFF;
		}
		
		Task Insert(int divisor, int remainder) {
			return Task.Factory.StartNew (() => {
				int r;
				r = 0;
				for (int i = 0; i < size; i++) {
					int key = r % size;
					if (key % divisor == remainder) {
						tree.Insert (key, "yes");
					}
					r = Pseudorandom(r);
				}
			});
		}
		
		Task Find(int divisor, int remainder) {
			return Task.Factory.StartNew (() => {
				int r;
				r = 0;
				for (int i = 0; i < size; i++) {
					int key = r % size;
					if (key % divisor == remainder) {
						if ("no".Equals(tree.Find (key, "no"))) {
							Console.WriteLine(key + " not found");
						}
					}
					r = Pseudorandom(r);
				}
			});
		}
		
		Task Delete(int divisor, int remainder) {
			return Task.Factory.StartNew (() => {
				int r;
				r = 0;
				for (int i = 0; i < size; i++) {
					int key = r % size;
					if (key % divisor == remainder) {
						tree.Delete (key);
					}
					r = Pseudorandom(r);
				}
			});
		}
		
		void Run ()
		{
			Console.WriteLine ("=== " + size + " iteration(s) by " + parallelism + " thread(s) ===");
			long t0, t1, t2, t3;
			Task[] tasks = new Task[parallelism];
			
			t0 = DateTime.Now.Ticks;
			
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i] = Insert (tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i].Wait ();
			}
			
			t1 = DateTime.Now.Ticks;
			insertTime = (t1 - t0) * 0.0000001;
			Console.WriteLine ("insert: " + insertTime + "s");
			
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i] = Find (tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i].Wait ();
			}
			
			t2 = DateTime.Now.Ticks;
			findTime = (t2 - t1) * 0.0000001;
			Console.WriteLine ("find: " + findTime + "s");
			
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i] = Delete (tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++) {
				tasks [i].Wait ();
			}
			
			t3 = DateTime.Now.Ticks;
			deleteTime = (t3 - t2) * 0.0000001;
			Console.WriteLine("delete: " + deleteTime + "s");
			
			totalTime = (t3 - t0) * 0.0000001;
			Console.WriteLine("total: " + totalTime + "s");
		}
	}
}
