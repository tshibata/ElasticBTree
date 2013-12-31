/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;
using System.Threading.Tasks;

using ElasticBTree;

namespace Benchmark
{
	class Benchmark
	{
		int size;

		int parallelism;

		ElasticBTree<int, string> tree = new ElasticBTree<int, string> ();

		Benchmark (int size, int parallelism)
		{
			this.size = size;
			this.parallelism = parallelism;
		}

		public static void Main (string[] args)
		{
			for (int i = 1; i <= 8; i *= 2) {
				Benchmark benchmark = new Benchmark (10 * 1000 * 1000, i);
				benchmark.Run ();
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

			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i] = Insert(tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i].Wait();
			}

			t1 = DateTime.Now.Ticks;
			Console.WriteLine("insert: " + ((t1 - t0) / 10000000F) + "s");

			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i] = Find(tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i].Wait();
			}

			t2 = DateTime.Now.Ticks;
			Console.WriteLine("find: " + ((t2 - t1) / 10000000F) + "s");

			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i] = Delete(tasks.Length, i);
			}
			for (int i = 0; i < tasks.Length; i ++)
			{
				tasks[i].Wait();
			}

			t3 = DateTime.Now.Ticks;
			Console.WriteLine("delete: " + ((t3 - t2) / 10000000F) + "s");

			Console.WriteLine("total: " + ((t3 - t0) / 10000000F) + "s");
		}
	}
}
