/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;

namespace ElasticBTree
{
	/*
	 * this class holds a reference to the root node and maintains height of the tree.
	 */
	internal class Ground<K, V>: Arbitrator<K, V>
		where K: IComparable
	{
		internal Ground ()
		{
			this.keys = new K[0];
			this.children = new Node<K,V>[]{new Leaf<K, V>(new K[0], new V[0])};
		}

		internal override Branch<K, V> Split ()
		{
			throw new NotSupportedException();
		}
		
		internal override void Join(K key, Node<K, V> node)
		{
			throw new NotSupportedException();
		}

		protected override int Fix (K key, int index)
		{
#if DEBUG
			if (index != 0) {
				throw new Exception("internal error");
			}
#endif
			children[index].Enter();
			// children[index] must be already locked.
			Node<K, V> original = children [index];
			try {
				if (children [index] is Branch<K, V>) {
					Branch<K, V> branch = (Branch<K, V>)children [index];
					if (branch.keys.Length < 1) {
						children [index] = branch.children [0];
					}
				}
				if (MaxKeys < children [index].keys.Length) {
					children [index] = children [index].Split ();
				}
				if (original != children [index]) {
					original.Exit();
					children[index].Enter();
				}
			} catch (Exception ex) {
				original.Exit();
				throw ex;
			}
			return 0;
		}
	}
}

