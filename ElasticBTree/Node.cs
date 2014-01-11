/*
 * Copyright(c) 2013-2014 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;

namespace ElasticBTree
{
	internal abstract class Node<K, V>
		where K: IComparable
	{
		/*
		 * number of keys a node should have at least
		 */
		public const int MinKeys = 2;

		/*
		 * number of keys a node should have at most
		 */
		public const int MaxKeys = 4;
		
		internal K[] keys;

		/*
		 * returns a new branch with two new children, reusing grandchildren.
		 */
		internal abstract Branch<K, V> Split();

		/*
		 * merge with next sibling.
		 */
		internal abstract void Join(K key, Node<K, V> node);

		volatile bool locked;

		internal void Enter ()
		{
			// following code is faster than Monitor.Enter(this) if there are enough cores
			while (locked) {
				Thread.SpinWait (1);
			}
			locked = true;
		}

		internal void Exit()
		{
			locked = false;
		}
	}
}

