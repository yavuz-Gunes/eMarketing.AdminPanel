    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using eMarketing.AdminPanel.Core;

    namespace eMarketing.AdminPanel.Componets
    {
        public class SidebarControl : Panel
        {
            public event Action<string> MenuClicked;

            private Dictionary<string, Panel> menuItems = new Dictionary<string, Panel>();
            private Panel activeIndicator;

            public SidebarControl()
            {
                Width = 240;
                Dock = DockStyle.Left;
                BackColor = AppColors.Sidebar;

                BuildSidebar();
            }

            private void BuildSidebar()
            {
                // Logo / Title
                var title = new Label
                {
                    Text = "eMarketing",
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = AppColors.TextPrimary,
                    Location = new Point(25, 30),
                    AutoSize = true
                };
                Controls.Add(title);

                // Active indicator (mavi bar)
                activeIndicator = new Panel
                {
                    Width = 4,
                    Height = 40,
                    BackColor = AppColors.Primary,
                    Visible = false
                };
                Controls.Add(activeIndicator);

                AddMenuItem("Dashboard", 100);
                AddMenuItem("Products", 150);
                AddMenuItem("Orders", 200);
                AddMenuItem("Customers", 250);

                SetActive("Dashboard");
            }

            private void AddMenuItem(string text, int top)
            {
                Panel container = new Panel
                {
                    Width = 240,
                    Height = 45,
                    Location = new Point(0, top),
                    Cursor = Cursors.Hand
                };

                Label lbl = new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = AppColors.TextSecondary,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(25, 0, 0, 0)
                };

                container.Controls.Add(lbl);

                container.MouseEnter += (s, e) =>
                {
                    if (!IsActive(text))
                        container.BackColor = Color.FromArgb(240, 242, 245);
                };

                container.MouseLeave += (s, e) =>
                {
                    if (!IsActive(text))
                        container.BackColor = Color.Transparent;
                };

                container.Click += (s, e) =>
                {
                    SetActive(text);
                    MenuClicked?.Invoke(text);
                };

                lbl.Click += (s, e) =>
                {
                    SetActive(text);
                    MenuClicked?.Invoke(text);
                };

                Controls.Add(container);
                menuItems[text] = container;
            }

            private void SetActive(string key)
            {
                foreach (var item in menuItems)
                {
                    item.Value.BackColor = Color.Transparent;
                    item.Value.Controls[0].ForeColor = AppColors.TextSecondary;
                }

                if (menuItems.ContainsKey(key))
                {
                    var panel = menuItems[key];
                    panel.BackColor = Color.FromArgb(230, 238, 255);
                    panel.Controls[0].ForeColor = AppColors.Primary;

                    activeIndicator.Location = new Point(0, panel.Top);
                    activeIndicator.Height = panel.Height;
                    activeIndicator.Visible = true;
                }
            }

            private bool IsActive(string key)
            {
                if (!menuItems.ContainsKey(key))
                    return false;

                return menuItems[key].BackColor == Color.FromArgb(230, 238, 255);
            }
        }
    }