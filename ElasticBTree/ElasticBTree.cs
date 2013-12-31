/*
 * Copyright(c) 2013 tshibata <staatsschreiber@gmail.com>
 * Licensed under the Apache License, Version 2.0
 */
using System;
using System.Threading;

namespace ElasticBTree
{
	public class ElasticBTree<K, V>
		where K: IComparable
	{
		Ground<K, V> ground = new Ground<K, V>();

		public ElasticBTree ()
		{
		}

		Leaf<K, V> FindLeaf (K key)
		{
			Node<K, V> node = ground;
			try {
				while (node is Arbitrator<K, V>) {
					Node<K, V> chosen = ((Arbitrator<K, V>)node).Choose (key);
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
			Leaf<K, V> leaf = FindLeaf (key);
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
			Leaf<K, V> leaf = FindLeaf (key);
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
			Leaf<K, V> leaf = FindLeaf (key);
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

