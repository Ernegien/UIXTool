using UIXTool.Extensions;
using UIXTool.Formats.Uix;
using UIXTool.Formats.Xpr;

namespace UIXTool.Forms
{
    // TODO: allow swapping viewer background color to see textures that might be white

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        // TODO: export option from viewer too
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            // select the file(s), only handle XPR or UIX for now
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Supported Types|*.uix;*.xpr";
            ofd.CheckFileExists = true;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            // clear the tree view and viewer
            tvContents.Nodes.Clear();
            tvContents.BeginUpdate();
            viewerImage.BackgroundImage = null;

            // process each file
            foreach (var file in ofd.FileNames)
            {
                var extension = Path.GetExtension(file);
                if (extension.Equals(".xpr", StringComparison.OrdinalIgnoreCase))
                {
                    LoadXpr(file);
                }
                else if (extension.Equals(".uix", StringComparison.OrdinalIgnoreCase))
                {
                    LoadUix(file);
                }
            }

            // expand all by default and select the first node
            tvContents.ExpandAll();
            if (tvContents.Nodes.Count > 0)
            {
                tvContents.SelectedNode = tvContents.Nodes[0];
            }
            tvContents.EndUpdate();
        }

        private void LoadUix(string path)
        {
            // parse the file
            var uix = new Uix(path);

            var uixNode = tvContents.Nodes.Add(uix.Name, uix.ToolTip, uix);
            foreach (var item in uix.Items)
            {
                var itemNode = uixNode.Nodes.Add(item.Name, item.ToolTip, item);
                if (item.Xpr != null)
                {
                    var xprNode = itemNode.Nodes.Add(item.Xpr.Name, item.Xpr.ToolTip, item.Xpr);
                    foreach (var resource in item.Xpr.Resources)
                    {
                        var resourceNode = xprNode.Nodes.Add(resource.Name, resource.ToolTip, resource);
                        BuildTreeViewContextMenu(resourceNode);
                    }
                }
            }
        }

        private void LoadXpr(string path)
        {
            // parse the file
            var xpr = new Xpr(null, path);

            // add the root xpr node
            var xprNode = tvContents.Nodes.Add(xpr.Name, xpr.ToolTip, xpr);

            // loop through the resources
            foreach (var resource in xpr.Resources)
            {
                // add the resource to the tree view
                var rsrcNode = xprNode.Nodes.Add(resource.Name, resource.ToolTip, resource);

                // only support exporting 2D textures for now
                if (resource.Type == XprResourceType.Texture && resource.TextureDimensions == 2)
                {
                    BuildTreeViewContextMenu(rsrcNode);
                }
            }
        }

        private void BuildTreeViewContextMenu(TreeNode node)
        {
            ContextMenuStrip ctxMenu = new ContextMenuStrip();

            if (node.Tag is XprResource rsrc)
            {
                var exportItem = new ToolStripMenuItem("Export", null, TreeViewItemClick);
                exportItem.Tag = node.Tag;
                ctxMenu.Items.Add(exportItem);

                // for now, basic guard against unsupported formats
                if (rsrc.TextureFormat == XprTextureFormat.LU_IMAGE_A8R8G8B8 ||
                    rsrc.TextureFormat == XprTextureFormat.LU_IMAGE_X8R8G8B8 ||
                    rsrc.TextureFormat == XprTextureFormat.SZ_A8R8G8B8 ||
                    rsrc.TextureFormat == XprTextureFormat.SZ_X8R8G8B8 ||
                    rsrc.TextureFormat == XprTextureFormat.LU_IMAGE_A8B8G8R8 ||
                    rsrc.TextureFormat == XprTextureFormat.SZ_A8B8G8R8
                    )
                {
                    var updateItem = new ToolStripMenuItem("Update", null, TreeViewItemClick);
                    updateItem.Tag = node.Tag;
                    ctxMenu.Items.Add(updateItem);
                }
            }

            node.ContextMenuStrip = ctxMenu;
        }

        private void TreeViewItemClick(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                switch (menuItem.Tag)
                {
                    case XprResource texture:
                        {
                            if (menuItem.Text == "Export")
                            {
                                using SaveFileDialog sfd = new SaveFileDialog();
                                sfd.Filter = "PNG|*.png";
                                if (sfd.ShowDialog() != DialogResult.OK)
                                    break;

                                texture.Image?.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            else if (menuItem.Text == "Update")
                            {
                                using OpenFileDialog ofd = new OpenFileDialog();
                                ofd.Filter = "PNG|*.png";
                                if (ofd.ShowDialog() != DialogResult.OK)
                                    break;

                                texture.UpdateTexture(ofd.FileName);
                                viewerImage.BackgroundImage = texture.Image;
                            }
                        }

                        break;
                    case Xpr xpr:

                        break;
                    case Uix uix:

                        break;
                    case UixItem uixItem:

                        break;
                    default:

                        break;
                }
            }
        }

        // update viewer/meta tab contents based on selection
        private void tvContents_AfterSelect(object sender, TreeViewEventArgs e)
        {
            viewerImage.BackgroundImage = null;
            viewerImage.ContextMenuStrip = null;

            TreeNode node = tvContents.SelectedNode;
            switch (node.Tag)
            {
                case Xpr xpr:
                    // TODO: display all textures in a grid view?

                    break;
                case XprResource texture:

                    ContextMenuStrip ctxExport = new();
                    ctxExport.ItemClicked += ExportViewerImage;
                    ctxExport.Items.Add("Save as PNG");
                    viewerImage.ContextMenuStrip = ctxExport;
                    viewerImage.BackgroundImage = texture.Image;

                    break;
                case Uix uix:
                    break;
                case UixItem uixItem:
                    break;
                default:
                    break;
            }
        }

        private void ExportViewerImage(object? sender, EventArgs e)
        {
            if (viewerImage.BackgroundImage == null)
                return;

            using SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            viewerImage.BackgroundImage.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        // TODO: figure out how icons/text are linked to the texture resources
        private void viewerImage_Paint(object sender, PaintEventArgs e)
        {
            return;

            if (viewerImage.BackgroundImage == null)
                return;

            var tag = tvContents?.SelectedNode?.Tag;
            if (tag is not XprResource)
                return;

            // only do for primary resource?
            XprResource resource = (XprResource)tag;
            //if (resource.DataOffset != 0)
            //    return;

            // get the uix item associated with it if possible
            UixItem? item = resource?.Parent?.Parent;
            if (item == null)
                return;

            using var gfx = Graphics.FromImage(viewerImage.BackgroundImage);
            using var pen = new Pen(Color.Black, 1);
            using var font = new Font("Tahoma", 20);
            using var brush = new SolidBrush(Color.Black);

            foreach (var meta in item.Metas)
            {
                if (meta.Data == null)
                    continue;

                var pos = meta.Data;

                // TODO: scale to tex dimensions for ones that are out of bounds?
                // TODO: one of the meta entries might be the bounding box for the rest?

                // TODO: get associated string/icon

                var rect = new Rectangle(pos.X, pos.Y, pos.Width, pos.Height);
                gfx.DrawRectangle(pen, rect);
                gfx.DrawString(meta.Id.ToString(), font, brush, rect);
            }
        }
    }
}