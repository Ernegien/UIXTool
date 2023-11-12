namespace UIXTool.Extensions
{
    public static class UiExtensions
    {
        /// <summary>
        /// Adds a tree node to the collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="name">The name.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>Returns the created child node.</returns>
        public static TreeNode Add(this TreeNodeCollection collection, string name, string tooltip, object tag)
        {
            var child = new TreeNode(name)
            {
                Tag = tag,
                ToolTipText = tooltip
            };
            collection.Add(child);
            return child;
        }
    }
}
