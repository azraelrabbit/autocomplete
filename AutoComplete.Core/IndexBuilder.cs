﻿using AutoComplete.Core.DataSource;

using AutoComplete.Core.DataStructure;
using AutoComplete.Core.Helpers;
using System.Collections.Generic;
using System.IO;

namespace AutoComplete.Core
{
    public class IndexBuilder : IIndexBuilder
    {
        private TrieIndexHeader _header;
        private Trie _trie;
        private Stream _headerStream;
        private Stream _indexStream;

        public IndexBuilder(Stream headerStream, Stream indexStream)
        {
            _headerStream = headerStream;
            _indexStream = indexStream;

            _header = new TrieIndexHeader();
            _trie = new Trie();
        }

        public IndexBuilder Add(string keyword)
        {
            _trie.Add(TrieNodeInput.Create(keyword));
            return this;
        }

        public IndexBuilder AddRange(IEnumerable<string> keywords)
        {
            if (keywords != null)
            {
                foreach (var item in keywords)
                {
                    Add(item);
                }
            }

            return this;
        }

        public IndexBuilder WithDataSource(IKeywordDataSource keywordDataSource)
        {
            _trie.Load(keywordDataSource);
            return this;
        }

        /// <summary>
        /// Dont forget to close streams after read.
        /// </summary>
        /// <param name="headerStream"></param>
        /// <param name="indexStream"></param>
        /// <returns>Processed node count</returns>
        public int Build()
        {
            PrepareForBuild();

            TrieSerializer.SerializeHeaderWithXmlSerializer(_headerStream, _header);
            var processedNodeCount = TrieSerializer.SerializeIndexWithBinaryWriter(_trie.Root, _header, _indexStream);

            return processedNodeCount;
        }

        private void PrepareForBuild()
        {
            ReorderTrieAndLoadHeader(_trie.Root);
        }

        private void ReorderTrieAndLoadHeader(TrieNode node)
        {
            TrieIndexHeader header = new TrieIndexHeader();
            Queue<TrieNode> indexerQueue = new Queue<TrieNode>();

            int order = 0;
            var builder = new TrieIndexHeaderBuilder();
            while (node != null)
            {
                node.Order = order;
                builder.AddChar(node.Character);

                // set parent's children index when current node's child
                // index not equal to zero and current index is not the root
                if (node.Parent != null && node.ChildIndex == 0)
                {
                    node.Parent.ChildrenCount = (node.Order - node.Parent.Order);
                }

                if (node.Children != null)
                {
                    int childIndex = 0;

                    foreach (var childNode in node.Children)
                    {
                        childNode.Value.ChildIndex = childIndex++;
                        indexerQueue.Enqueue(childNode.Value);
                    }
                }

                ++order;

                if (indexerQueue.Count == 0)
                    break;

                node = indexerQueue.Dequeue();
            }

            _header = builder.Build();
        }
    }
}