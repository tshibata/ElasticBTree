/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;

namespace POBTree
{
	internal class Branch<K, V>: Arbitrator<K, V>
		where K: IComparable
	{
		internal Branch (K[] keys, Node<K, V>[] children)
		{
			this.keys = keys;
			this.children = children;
		}

		internal override Branch<K, V> Split()
		{
			int index = keys.Length / 2;
			K[] leftKeys = new K[index];
			Node<K, V>[] leftChildren = new Node<K, V>[index + 1];
			for (int i = 0; i < index; i++) {
				leftKeys [i] = keys [i];
				leftChildren [i] = children [i];
			}
			leftChildren[index] = children[index];
			K[] rightKeys = new K[keys.Length - 1 - index];
			Node<K, V>[] rightChildren = new Node<K, V>[keys.Length - index];
			for (int i = 0; i < rightKeys.Length; i++) {
				rightKeys[i] = keys [index + 1 + i];
				rightChildren[i] = children [index + 1 + i];
			}
			rightChildren[rightKeys.Length] = children [index + 1 + rightKeys.Length];
			return new Branch<K, V>(
				new K[]{keys[index]},
				new Node<K, V>[]{
					new Branch<K, V>(leftKeys, leftChildren),
					new Branch<K, V>(rightKeys, rightChildren)
				}
			);
		}

		internal override void Join (K key, Node<K, V> node)
		{
			Branch<K, V> branch = (Branch<K, V>) node;
			K[] oldKeys = keys;
			keys = new K[oldKeys.Length + 1 + branch.keys.Length];
			for (int i = 0; i < oldKeys.Length; i++) {
				keys [i] = oldKeys [i];
			}
			keys[oldKeys.Length] = key;
			for (int i = 0; i < branch.keys.Length; i++) {
				keys [oldKeys.Length + 1 + i] = branch.keys [i];
			}
			Node<K, V>[] oldChildren = children;
			children = new Node<K, V>[oldChildren.Length + branch.children.Length];
			for (int i = 0; i < oldChildren.Length; i++) {
				children [i] = oldChildren [i];
			}
			for (int i = 0; i < branch.children.Length; i++) {
				children [oldChildren.Length + i] = branch.children [i];
			}
		}

		void JoinChildren(int index)
		{
			children [index].Join (keys [index], children [index + 1]);
			K[] oldKeys = keys;
			keys = new K[oldKeys.Length - 1];
			Node<K, V>[] oldChildren = children;
			children = new Node<K, V>[oldChildren.Length - 1];
			for (int i = 0; i < index; i++) {
				children [i] = oldChildren [i];
				keys [i] = oldKeys [i];
			}
			children [index] = oldChildren [index];
			for (int i = index; i < keys.Length; i++) {
				keys [i] = oldKeys [i + 1];
				children [i + 1] = oldChildren [i + 2];
			}
		}
		
		void SplitChildren (int index)
		{
			Branch<K, V> node = children [index].Split ();
			K[] oldKeys = keys;
			keys = new K[oldKeys.Length + 1];
			Node<K, V>[] oldChildren = children;
			children = new Node<K, V>[oldChildren.Length + 1];
			for (int j = 0; j < index; j++) {
				children [j] = oldChildren [j];
				keys [j] = oldKeys [j];
			}
			children [index] = node.children [0];
			keys [index] = node.keys [0];
			children [index + 1] = node.children [1];
			for (int j = index + 1; j < keys.Length; j++) {
				keys [j] = oldKeys [j - 1];
				children [j + 1] = oldChildren [j];
			}
		}
		
		protected override int Fix (K key, int index)
		{
			Node<K, V> chosen, original, neighbor, newbie;
			chosen = original = children [index];
			original.Enter ();
			neighbor = null;
			newbie = null;
			try {
				if (children [index].keys.Length < MinKeys) {
					// too narrow
					if (index == 0) {
						// children[index] is already locked
						neighbor = children [index + 1];
						neighbor.Enter ();
						// children[index] and children[index + 1] are now locked.
						JoinChildren(index);
						neighbor.Exit();
						neighbor = null;
					} else {
						index = index - 1;
						// children[index + 1] is already locked
						neighbor = children [index];
						neighbor.Enter ();
						// children[index] and children[index + 1] are now locked.
						JoinChildren(index);
						chosen = neighbor;
						original.Exit();
						original = null;
					}
				}
				if (MaxKeys < children [index].keys.Length) {
					// too wide
					SplitChildren(index);
					if (key.CompareTo (keys [index]) < 0) {
						chosen = newbie = children[index];
						newbie.Enter();
					} else {
						index = index + 1;
						chosen = newbie = children[index];
						newbie.Enter();
					}
				}
			} catch (Exception ex) {
				chosen.Exit ();
				throw ex;
			} finally {
				if (original != null && original != chosen) {
					original.Exit ();
				}
				if (neighbor != null && neighbor != chosen) {
					neighbor.Exit ();
				}
				if (newbie != null && newbie != chosen) {
					newbie.Exit ();
				}
			}
			return index;
		}
	}
}

