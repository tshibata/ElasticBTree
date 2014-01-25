/*
 * Copyright(c) 2013-2014 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;

namespace POBTree
{
	public class POBTree<K, V>
		where K: IComparable
	{
		Ground<K, V> ground = new Ground<K, V>();

		public POBTree ()
		{
		}

		Leaf<K, V> FindLeaf (K key, bool fix)
		{
			Node<K, V> node = ground;
			try {
				while (node is Arbitrator<K, V>) {
					Node<K, V> chosen = ((Arbitrator<K, V>)node).Choose (key, fix);
					node.Exit ();
					node = chosen;
				}
			} catch (Exception ex) {
				node.Exit ();
				throw ex;
			}
			return (Leaf<K, V>)node;
		}
		
		public V Find (K key, V fallback)
		{
			lock (this) {
				ground.Enter ();
			}
			Leaf<K, V> leaf = FindLeaf (key, false);
			try {
				V value = leaf.Find (key, fallback);
				return value;
			} finally {
				leaf.Exit ();
			}
		}
		
		public void Insert (K key, V value)
		{
			lock (this) {
				ground.Enter ();
			}
			Leaf<K, V> leaf = FindLeaf (key, true);
			try {
				leaf.Insert (key, value);
			} finally {
				leaf.Exit ();
			}
		}
		
		public void Delete (K key)
		{
			lock (this) {
				ground.Enter ();
			}
			Leaf<K, V> leaf = FindLeaf (key, true);
			try {
				leaf.Delete (key);
			} finally {
				leaf.Exit ();
			}
		}

		public override string ToString ()
		{
			return ground.ToString();
		}
	}
}

