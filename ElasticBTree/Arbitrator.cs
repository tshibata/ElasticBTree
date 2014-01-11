/*
 * Copyright(c) 2013-2014 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;

namespace ElasticBTree
{
	internal abstract class Arbitrator<K, V>: Node<K, V>
		where K: IComparable
	{
		internal Node<K, V>[] children;
		// children[0] < keys[0] <= children[1] < key[1] ...
		
		public Arbitrator ()
		{
		}

		/*
		 * fix the child.
		 */
		protected abstract int Fix (K key, int index);

		internal Node<K, V> Choose (K key, bool fix)
		{
			int index;
			for (index = 0; index < keys.Length; index++) {
				if (key.CompareTo (keys [index]) < 0) {
					break;
				}
			}
			if (fix) {
				index = Fix (key, index);
			}
			return children[index];
		}
		
		public override string ToString ()
		{
			lock (this) {
				string s = "(" + children [0];
				for (int i = 0; i < keys.Length; i++) {
					s += keys [i];
					s += children [i + 1];
				}
				return s + ")";
			}
		}
	}
}

