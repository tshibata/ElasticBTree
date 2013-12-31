/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

using ElasticBTree;

namespace Test
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void Empty ()
		{
			ElasticBTree<int, string> tree = new ElasticBTree<int, string>();
			Assert.AreEqual("x", tree.Find(0, "x"));
		}
		
		[Test()]
		public void One ()
		{
			ElasticBTree<int, string> tree = new ElasticBTree<int, string>();
			tree.Insert(1, "one");
			Assert.AreEqual("one", tree.Find(1, "x"));
		}
		
		void ArrayTest (ElasticBTree<int, string> tree, int[] a)
		{
			HashSet<int> set = new HashSet<int> ();
			for (int i = 0; i < a.Length; i++) {
				int key = a[i];
				if (set.Contains (key)) {
					set.Remove (key);
					tree.Delete (key);
				} else {
					set.Add (key);
					tree.Insert (key, key.ToString ());
				}
			}
			foreach (int key in set) {
				Assert.AreEqual (key.ToString (), tree.Find (key, "x"));
				tree.Delete (key);
				Assert.AreEqual ("x", tree.Find (key, "x"));
			}
		}

		[TestCase(10)]
		[TestCase(100)]
		[TestCase(1000)]
		public void Pseudorandom (int limit)
		{
			ElasticBTree<int, string> tree = new ElasticBTree<int, string> ();
			int[] a = new int[1000];
			int random;
			random = 0;
			for (int i = 0; i < a.Length; i++) {
				a[i] = random % limit;
				random = (random * 1103515245 + 12345) & 0x7FFFFFFF;
			}
			ArrayTest(tree, a);
		}

		[TestCase(10)]
		[TestCase(100)]
		[TestCase(1000)]
		public void Parallel (int limit)
		{
			ElasticBTree<int, string> tree = new ElasticBTree<int, string> ();
			int[] evenNumbers = new int[500];
			int[] oddNumbers = new int[500];
			int random;
			random = 0;
			for (int i = 0; i < evenNumbers.Length; i++) {
				evenNumbers[i] = (random % limit) * 2;
				random = (random * 1103515245 + 12345) & 0x7FFFFFFF;
			}
			for (int i = 0; i < evenNumbers.Length; i++) {
				oddNumbers[i] = (random % limit) * 2 + 1;
				random = (random * 1103515245 + 12345) & 0x7FFFFFFF;
			}

			Task task1 = Task.Factory.StartNew (() => {
				ArrayTest(tree, evenNumbers);
			});
			Task task2 = Task.Factory.StartNew (() => {
				ArrayTest(tree, oddNumbers);
			});
			
			task1.Wait ();
			task2.Wait ();
		}
	}
}

