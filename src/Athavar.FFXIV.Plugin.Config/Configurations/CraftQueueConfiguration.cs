// <copyright file="CraftQueueConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

public class CraftQueueConfiguration : BasicModuleConfig
{
    /// <summary>
    ///     Gets the root folder.
    /// </summary>
    public FolderNode RootFolder { get; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets a value indicating whether to wait after a craft actions.
    /// </summary>
    public bool CraftWaitSkip { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to skip quality increasing actions when at 100% HQ chance.
    /// </summary>
    public bool QualitySkip { get; set; } = false;

    /// <summary>
    ///     Get all nodes in the tree.
    /// </summary>
    /// <returns>All the nodes.</returns>
    public IEnumerable<INode> GetAllNodes() => new INode[] { this.RootFolder }.Concat(this.GetAllNodes(this.RootFolder.Children));

    /// <summary>
    ///     Tries to find the parent of a node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="parent">Parent of the node or null.</param>
    /// <returns>A value indicating whether the parent was found.</returns>
    public bool TryFindParent(INode node, out FolderNode? parent)
    {
        foreach (var candidate in this.GetAllNodes())
        {
            if (candidate is FolderNode folder && folder.Children.Contains(node))
            {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }

    /// <summary>
    ///     Gets all the nodes in this subset of the tree.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The nodes in the tree.</returns>
    private IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is FolderNode folder)
            {
                var childNodes = this.GetAllNodes(folder.Children);
                foreach (var childNode in childNodes)
                {
                    yield return childNode;
                }
            }
        }
    }
}