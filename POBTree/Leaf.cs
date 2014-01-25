/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;

namespace POBTree
{
	internal class Leaf<K, V>: Node<K, V>
		where K: IComparable
	{
		internal V[] values;

		public Leaf (K[] keys, V[] values)
		{
			this.keys = keys;
			this.values = values;
		}

		internal override Branch<K, V> Split ()
		{
			int index = keys.Length / 2;
			K[] leftKeys = new K[index];
			V[] leftValues = new V[index];
			for (int i = 0; i < index; i++) {
				leftKeys [i] = keys [i];
				leftValues [i] = values [i];
			}
			K[] rightKeys = new K[keys.Length - index];
			V[] rightValues = new V[keys.Length - index];
			for (int i = 0; i < keys.Length - index; i++) {
				rightKeys[i] = keys[index + i];
				rightValues[i] = values[index + i];
			}
			return new Branch<K, V>(
				new K[]{keys[index]},
				new Node<K, V>[]{
					new Leaf<K, V>(leftKeys, leftValues),
					new Leaf<K, V>(rightKeys, rightValues)
				}
			);
		}

		internal override void Join (K key, Node<K, V> node)
		{
			Leaf<K, V> leaf = (Leaf<K, V>) node;
			K[] oldKeys = keys;
			keys = new K[leaf.keys.Length + keys.Length];
			for (int i = 0; i < oldKeys.Length; i++) {
				keys [i] = oldKeys [i];
			}
			for (int i = 0; i < leaf.keys.Length; i++) {
				keys [oldKeys.Length + i] = leaf.keys [i];
			}
			V[] oldValues = values;
			values = new V[leaf.keys.Length + values.Length];
			for (int i = 0; i < oldValues.Length; i++) {
				values [i] = oldValues [i];
			}
			for (int i = 0; i < leaf.keys.Length; i++) {
				values [oldValues.Length + i] = leaf.values [i];
			}
		}

		internal V Find (K key, V fallback)
		{
			int index;
			for (index = 0; index < keys.Length; index++) {
				if (key.CompareTo (keys [index]) == 0) {
					return values[index];
				}
			}
			return fallback;
		}
		
		internal void Insert (K key, V value)
		{
			int index;
			for (index = 0; index < keys.Length; index++) {
				if (key.CompareTo (keys [index]) < 0) {
					break;
				}
				if (key.CompareTo (keys [index]) == 0) {
					// replace
					keys [index] = key;
					values [index] = value;
					return;
				}
			}
			// insert
			K[] oldKeys = keys;
			keys = new K[keys.Length + 1];
			V[] oldValues = values;
			values = new V[values.Length + 1];
			for (int i = 0; i < index; i++) {
				keys [i] = oldKeys [i];
				values [i] = oldValues [i];
			}
			keys [index] = key;
			values [index] = value;
			for (int i = index; i < oldKeys.Length; i++) {
				keys [i + 1] = oldKeys [i];
				values [i + 1] = oldValues [i];
			}
		}
		
		internal void Delete (K key)
		{
			int index;
			for (index = 0; index < keys.Length; index++) {
				if (key.CompareTo (keys [index]) < 0) {
					break;
				}
				if (key.CompareTo (keys [index]) == 0) {
					// delete
					K[] oldKeys = keys;
					keys = new K[keys.Length - 1];
					V[] oldValues = values;
					values = new V[values.Length - 1];
					for (int i = 0; i < index; i++) {
						keys [i] = oldKeys [i];
						values [i] = oldValues [i];
					}
					for (int i = index + 1; i < oldKeys.Length; i++) {
						keys [i - 1] = oldKeys [i];
						values [i - 1] = oldValues [i];
					}
					return;
				}
			}
			// nothing to do
		}

		public override string ToString ()
		{
			lock (this) {
				string s = "( ";
				foreach (K key in keys) {
					s += key + " ";
				}
				return s + ")";
			}
		}
	}
}

