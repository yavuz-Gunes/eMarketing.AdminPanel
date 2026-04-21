namespace eMarketing.AdminPanel.Forms
{
    partial class FrmProducts
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnNewProduct = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chkDarkMode = new System.Windows.Forms.CheckBox();
            this.pnlProductCard = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtProductName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtStock = new System.Windows.Forms.TextBox();
            this.labelCategory = new System.Windows.Forms.Label();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtImageUrl = new System.Windows.Forms.TextBox();
            this.chkIsActive = new System.Windows.Forms.CheckBox();
            this.txtProductId = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.pnlGrid = new System.Windows.Forms.Panel();
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.pnlSummaryCard = new System.Windows.Forms.Panel();
            this.pnlCardPassive = new System.Windows.Forms.Panel();
            this.lblCardPassiveTitle = new System.Windows.Forms.Label();
            this.lblCardPassiveValue = new System.Windows.Forms.Label();
            this.pnlCardLowStock = new System.Windows.Forms.Panel();
            this.lblCardLowTitle = new System.Windows.Forms.Label();
            this.lblCardLowValue = new System.Windows.Forms.Label();
            this.pnlCardActive = new System.Windows.Forms.Panel();
            this.lblCardActiveTitle = new System.Windows.Forms.Label();
            this.lblCardActiveValue = new System.Windows.Forms.Label();
            this.pnlCardTotal = new System.Windows.Forms.Panel();
            this.lblCardTotalTitle = new System.Windows.Forms.Label();
            this.lblCardTotalValue = new System.Windows.Forms.Label();
            this.pnlFilterBar = new System.Windows.Forms.Panel();
            this.cmbFilterCategory = new System.Windows.Forms.ComboBox();
            this.cmbFilterStockStatus = new System.Windows.Forms.ComboBox();
            this.cmbFilterActive = new System.Windows.Forms.ComboBox();
            this.pnlTopBar.SuspendLayout();
            this.pnlProductCard.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.pnlSummaryCard.SuspendLayout();
            this.pnlCardPassive.SuspendLayout();
            this.pnlCardLowStock.SuspendLayout();
            this.pnlCardActive.SuspendLayout();
            this.pnlCardTotal.SuspendLayout();
            this.pnlFilterBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTopBar
            // 
            this.pnlTopBar.Controls.Add(this.lblTitle);
            this.pnlTopBar.Controls.Add(this.btnNewProduct);
            this.pnlTopBar.Controls.Add(this.txtSearch);
            this.pnlTopBar.Controls.Add(this.btnRefresh);
            this.pnlTopBar.Controls.Add(this.chkDarkMode);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopBar.Location = new System.Drawing.Point(0, 0);
            this.pnlTopBar.Name = "pnlTopBar";
            this.pnlTopBar.Padding = new System.Windows.Forms.Padding(12);
            this.pnlTopBar.Size = new System.Drawing.Size(1200, 64);
            this.pnlTopBar.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(16, 16);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(201, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "�r�n Y�netimi";
            // 
            // btnNewProduct
            // 
            this.btnNewProduct.Location = new System.Drawing.Point(220, 16);
            this.btnNewProduct.Name = "btnNewProduct";
            this.btnNewProduct.Size = new System.Drawing.Size(110, 32);
            this.btnNewProduct.TabIndex = 1;
            this.btnNewProduct.Text = "Yeni �r�n";
            this.btnNewProduct.UseVisualStyleBackColor = true;
            this.btnNewProduct.Click += new System.EventHandler(this.btnNewProduct_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(1680, 20);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(260, 22);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(1948, 16);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 32);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Yenile";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // chkDarkMode
            // 
            this.chkDarkMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDarkMode.Location = new System.Drawing.Point(2036, 20);
            this.chkDarkMode.Name = "chkDarkMode";
            this.chkDarkMode.Size = new System.Drawing.Size(100, 24);
            this.chkDarkMode.TabIndex = 4;
            this.chkDarkMode.Text = "Dark";
            this.chkDarkMode.UseVisualStyleBackColor = true;
            this.chkDarkMode.CheckedChanged += new System.EventHandler(this.chkDarkMode_CheckedChanged);
            // 
            // pnlProductCard
            // 
            this.pnlProductCard.Controls.Add(this.label1);
            this.pnlProductCard.Controls.Add(this.txtProductName);
            this.pnlProductCard.Controls.Add(this.label2);
            this.pnlProductCard.Controls.Add(this.txtDescription);
            this.pnlProductCard.Controls.Add(this.label3);
            this.pnlProductCard.Controls.Add(this.txtPrice);
            this.pnlProductCard.Controls.Add(this.label4);
            this.pnlProductCard.Controls.Add(this.txtStock);
            this.pnlProductCard.Controls.Add(this.labelCategory);
            this.pnlProductCard.Controls.Add(this.cmbCategory);
            this.pnlProductCard.Controls.Add(this.label5);
            this.pnlProductCard.Controls.Add(this.txtImageUrl);
            this.pnlProductCard.Controls.Add(this.chkIsActive);
            this.pnlProductCard.Controls.Add(this.txtProductId);
            this.pnlProductCard.Controls.Add(this.btnAdd);
            this.pnlProductCard.Controls.Add(this.btnUpdate);
            this.pnlProductCard.Controls.Add(this.btnDelete);
            this.pnlProductCard.Controls.Add(this.btnClear);
            this.pnlProductCard.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlProductCard.Location = new System.Drawing.Point(0, 64);
            this.pnlProductCard.Name = "pnlProductCard";
            this.pnlProductCard.Padding = new System.Windows.Forms.Padding(16);
            this.pnlProductCard.Size = new System.Drawing.Size(360, 656);
            this.pnlProductCard.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "�r�n Ad";
            // 
            // txtProductName
            // 
            this.txtProductName.Location = new System.Drawing.Point(16, 34);
            this.txtProductName.Name = "txtProductName";
            this.txtProductName.Size = new System.Drawing.Size(320, 22);
            this.txtProductName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "A��klama";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(16, 88);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(320, 60);
            this.txtDescription.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 156);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Fiyat";
            // 
            // txtPrice
            // 
            this.txtPrice.Location = new System.Drawing.Point(16, 178);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(140, 22);
            this.txtPrice.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(176, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Stok";
            // 
            // txtStock
            // 
            this.txtStock.Location = new System.Drawing.Point(176, 178);
            this.txtStock.Name = "txtStock";
            this.txtStock.Size = new System.Drawing.Size(160, 22);
            this.txtStock.TabIndex = 5;
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.Location = new System.Drawing.Point(12, 210);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(57, 16);
            this.labelCategory.TabIndex = 1;
            this.labelCategory.Text = "Kategori";
            // 
            // cmbCategory
            // 
            this.cmbCategory.Location = new System.Drawing.Point(16, 232);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(200, 24);
            this.cmbCategory.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 264);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "G�rsel URL";
            // 
            // txtImageUrl
            // 
            this.txtImageUrl.Location = new System.Drawing.Point(16, 286);
            this.txtImageUrl.Name = "txtImageUrl";
            this.txtImageUrl.Size = new System.Drawing.Size(320, 22);
            this.txtImageUrl.TabIndex = 7;
            // 
            // chkIsActive
            // 
            this.chkIsActive.AutoSize = true;
            this.chkIsActive.Location = new System.Drawing.Point(16, 318);
            this.chkIsActive.Name = "chkIsActive";
            this.chkIsActive.Size = new System.Drawing.Size(54, 20);
            this.chkIsActive.TabIndex = 8;
            this.chkIsActive.Text = "Aktif";
            this.chkIsActive.UseVisualStyleBackColor = true;
            // 
            // txtProductId
            // 
            this.txtProductId.Location = new System.Drawing.Point(240, 318);
            this.txtProductId.Name = "txtProductId";
            this.txtProductId.Size = new System.Drawing.Size(96, 22);
            this.txtProductId.TabIndex = 9;
            this.txtProductId.Visible = false;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(16, 356);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 34);
            this.btnAdd.TabIndex = 10;
            this.btnAdd.Text = "Ekle";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(104, 356);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(80, 34);
            this.btnUpdate.TabIndex = 11;
            this.btnUpdate.Text = "G?ncelle";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(192, 356);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(80, 34);
            this.btnDelete.TabIndex = 12;
            this.btnDelete.Text = "Sil";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(280, 356);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(56, 34);
            this.btnClear.TabIndex = 13;
            this.btnClear.Text = "Temizle";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // pnlContent
            // 
            this.pnlContent.Controls.Add(this.pnlGrid);
            this.pnlContent.Controls.Add(this.pnlSummaryCard);
            this.pnlContent.Controls.Add(this.pnlFilterBar);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(360, 64);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(12);
            this.pnlContent.Size = new System.Drawing.Size(840, 656);
            this.pnlContent.TabIndex = 0;
            // 
            // pnlGrid
            // 
            this.pnlGrid.Controls.Add(this.dgvProducts);
            this.pnlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGrid.Location = new System.Drawing.Point(12, 148);
            this.pnlGrid.Name = "pnlGrid";
            this.pnlGrid.Padding = new System.Windows.Forms.Padding(8);
            this.pnlGrid.Size = new System.Drawing.Size(816, 496);
            this.pnlGrid.TabIndex = 0;
            // 
            // dgvProducts
            // 
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProducts.Location = new System.Drawing.Point(8, 8);
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.RowHeadersWidth = 51;
            this.dgvProducts.RowTemplate.Height = 60;
            this.dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.Size = new System.Drawing.Size(800, 480);
            this.dgvProducts.TabIndex = 0;
            this.dgvProducts.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProducts_CellClick);
            this.dgvProducts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProducts_CellContentClick);
            this.dgvProducts.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProducts_CellClick);
            this.dgvProducts.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvProducts_CellFormatting);
            this.dgvProducts.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgvProducts_RowPrePaint);
            this.dgvProducts.SelectionChanged += new System.EventHandler(this.dgvProducts_SelectionChanged);
            // 
            // pnlSummaryCard
            // 
            this.pnlSummaryCard.Controls.Add(this.pnlCardPassive);
            this.pnlSummaryCard.Controls.Add(this.pnlCardLowStock);
            this.pnlSummaryCard.Controls.Add(this.pnlCardActive);
            this.pnlSummaryCard.Controls.Add(this.pnlCardTotal);
            this.pnlSummaryCard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSummaryCard.Location = new System.Drawing.Point(12, 60);
            this.pnlSummaryCard.Name = "pnlSummaryCard";
            this.pnlSummaryCard.Padding = new System.Windows.Forms.Padding(8);
            this.pnlSummaryCard.Size = new System.Drawing.Size(816, 88);
            this.pnlSummaryCard.TabIndex = 1;
            // 
            // pnlCardPassive
            // 
            this.pnlCardPassive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlCardPassive.Controls.Add(this.lblCardPassiveTitle);
            this.pnlCardPassive.Controls.Add(this.lblCardPassiveValue);
            this.pnlCardPassive.Location = new System.Drawing.Point(588, 8);
            this.pnlCardPassive.Name = "pnlCardPassive";
            this.pnlCardPassive.Padding = new System.Windows.Forms.Padding(8);
            this.pnlCardPassive.Size = new System.Drawing.Size(180, 72);
            this.pnlCardPassive.TabIndex = 0;
            // 
            // lblCardPassiveTitle
            // 
            this.lblCardPassiveTitle.AutoSize = true;
            this.lblCardPassiveTitle.Location = new System.Drawing.Point(8, 8);
            this.lblCardPassiveTitle.Name = "lblCardPassiveTitle";
            this.lblCardPassiveTitle.Size = new System.Drawing.Size(83, 16);
            this.lblCardPassiveTitle.TabIndex = 0;
            this.lblCardPassiveTitle.Text = "Pasif �r�nler";
            // 
            // lblCardPassiveValue
            // 
            this.lblCardPassiveValue.AutoSize = true;
            this.lblCardPassiveValue.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCardPassiveValue.Location = new System.Drawing.Point(8, 32);
            this.lblCardPassiveValue.Name = "lblCardPassiveValue";
            this.lblCardPassiveValue.Size = new System.Drawing.Size(28, 32);
            this.lblCardPassiveValue.TabIndex = 1;
            this.lblCardPassiveValue.Text = "0";
            // 
            // pnlCardLowStock
            // 
            this.pnlCardLowStock.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlCardLowStock.Controls.Add(this.lblCardLowTitle);
            this.pnlCardLowStock.Controls.Add(this.lblCardLowValue);
            this.pnlCardLowStock.Location = new System.Drawing.Point(396, 8);
            this.pnlCardLowStock.Name = "pnlCardLowStock";
            this.pnlCardLowStock.Padding = new System.Windows.Forms.Padding(8);
            this.pnlCardLowStock.Size = new System.Drawing.Size(180, 72);
            this.pnlCardLowStock.TabIndex = 1;
            // 
            // lblCardLowTitle
            // 
            this.lblCardLowTitle.AutoSize = true;
            this.lblCardLowTitle.Location = new System.Drawing.Point(8, 8);
            this.lblCardLowTitle.Name = "lblCardLowTitle";
            this.lblCardLowTitle.Size = new System.Drawing.Size(75, 16);
            this.lblCardLowTitle.TabIndex = 0;
            this.lblCardLowTitle.Text = "D???k Stok";
            //this.lblCardLowTitle.Click += new System.EventHandler(this.lblCardLowTitle_Click);
            // 
            // lblCardLowValue
            // 
            this.lblCardLowValue.AutoSize = true;
            this.lblCardLowValue.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCardLowValue.Location = new System.Drawing.Point(8, 32);
            this.lblCardLowValue.Name = "lblCardLowValue";
            this.lblCardLowValue.Size = new System.Drawing.Size(28, 32);
            this.lblCardLowValue.TabIndex = 1;
            this.lblCardLowValue.Text = "0";
            // 
            // pnlCardActive
            // 
            this.pnlCardActive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlCardActive.Controls.Add(this.lblCardActiveTitle);
            this.pnlCardActive.Controls.Add(this.lblCardActiveValue);
            this.pnlCardActive.Location = new System.Drawing.Point(204, 8);
            this.pnlCardActive.Name = "pnlCardActive";
            this.pnlCardActive.Padding = new System.Windows.Forms.Padding(8);
            this.pnlCardActive.Size = new System.Drawing.Size(180, 72);
            this.pnlCardActive.TabIndex = 2;
            // 
            // lblCardActiveTitle
            // 
            this.lblCardActiveTitle.AutoSize = true;
            this.lblCardActiveTitle.Location = new System.Drawing.Point(8, 8);
            this.lblCardActiveTitle.Name = "lblCardActiveTitle";
            this.lblCardActiveTitle.Size = new System.Drawing.Size(78, 16);
            this.lblCardActiveTitle.TabIndex = 0;
            this.lblCardActiveTitle.Text = "Aktif �r�nler";
            // 
            // lblCardActiveValue
            // 
            this.lblCardActiveValue.AutoSize = true;
            this.lblCardActiveValue.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCardActiveValue.Location = new System.Drawing.Point(8, 32);
            this.lblCardActiveValue.Name = "lblCardActiveValue";
            this.lblCardActiveValue.Size = new System.Drawing.Size(28, 32);
            this.lblCardActiveValue.TabIndex = 1;
            this.lblCardActiveValue.Text = "0";
            // 
            // pnlCardTotal
            // 
            this.pnlCardTotal.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlCardTotal.Controls.Add(this.lblCardTotalTitle);
            this.pnlCardTotal.Controls.Add(this.lblCardTotalValue);
            this.pnlCardTotal.Location = new System.Drawing.Point(12, 8);
            this.pnlCardTotal.Name = "pnlCardTotal";
            this.pnlCardTotal.Padding = new System.Windows.Forms.Padding(8);
            this.pnlCardTotal.Size = new System.Drawing.Size(180, 72);
            this.pnlCardTotal.TabIndex = 3;
            // 
            // lblCardTotalTitle
            // 
            this.lblCardTotalTitle.AutoSize = true;
            this.lblCardTotalTitle.Location = new System.Drawing.Point(8, 8);
            this.lblCardTotalTitle.Name = "lblCardTotalTitle";
            this.lblCardTotalTitle.Size = new System.Drawing.Size(100, 16);
            this.lblCardTotalTitle.TabIndex = 0;
            this.lblCardTotalTitle.Text = "Toplam �r�nler";
            // 
            // lblCardTotalValue
            // 
            this.lblCardTotalValue.AutoSize = true;
            this.lblCardTotalValue.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCardTotalValue.Location = new System.Drawing.Point(8, 32);
            this.lblCardTotalValue.Name = "lblCardTotalValue";
            this.lblCardTotalValue.Size = new System.Drawing.Size(28, 32);
            this.lblCardTotalValue.TabIndex = 1;
            this.lblCardTotalValue.Text = "0";
            // 
            // pnlFilterBar
            // 
            this.pnlFilterBar.Controls.Add(this.cmbFilterCategory);
            this.pnlFilterBar.Controls.Add(this.cmbFilterStockStatus);
            this.pnlFilterBar.Controls.Add(this.cmbFilterActive);
            this.pnlFilterBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilterBar.Location = new System.Drawing.Point(12, 12);
            this.pnlFilterBar.Name = "pnlFilterBar";
            this.pnlFilterBar.Padding = new System.Windows.Forms.Padding(8);
            this.pnlFilterBar.Size = new System.Drawing.Size(816, 48);
            this.pnlFilterBar.TabIndex = 2;
            // 
            // cmbFilterCategory
            // 
            this.cmbFilterCategory.Location = new System.Drawing.Point(12, 12);
            this.cmbFilterCategory.Name = "cmbFilterCategory";
            this.cmbFilterCategory.Size = new System.Drawing.Size(180, 24);
            this.cmbFilterCategory.TabIndex = 1;
            this.cmbFilterCategory.SelectedIndexChanged += new System.EventHandler(this.cmbFilterCategory_SelectedIndexChanged);
            // 
            // cmbFilterStockStatus
            // 
            this.cmbFilterStockStatus.Location = new System.Drawing.Point(200, 12);
            this.cmbFilterStockStatus.Name = "cmbFilterStockStatus";
            this.cmbFilterStockStatus.Size = new System.Drawing.Size(140, 24);
            this.cmbFilterStockStatus.TabIndex = 2;
            this.cmbFilterStockStatus.SelectedIndexChanged += new System.EventHandler(this.cmbFilterStockStatus_SelectedIndexChanged);
            // 
            // cmbFilterActive
            // 
            this.cmbFilterActive.Location = new System.Drawing.Point(352, 12);
            this.cmbFilterActive.Name = "cmbFilterActive";
            this.cmbFilterActive.Size = new System.Drawing.Size(120, 24);
            this.cmbFilterActive.TabIndex = 3;
            this.cmbFilterActive.SelectedIndexChanged += new System.EventHandler(this.cmbFilterActive_SelectedIndexChanged);
            // 
            // FrmProducts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 720);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlProductCard);
            this.Controls.Add(this.pnlTopBar);
            this.Name = "FrmProducts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FrmProducts_Load);
            this.pnlTopBar.ResumeLayout(false);
            this.pnlTopBar.PerformLayout();
            this.pnlProductCard.ResumeLayout(false);
            this.pnlProductCard.PerformLayout();
            this.pnlContent.ResumeLayout(false);
            this.pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.pnlSummaryCard.ResumeLayout(false);
            this.pnlCardPassive.ResumeLayout(false);
            this.pnlCardPassive.PerformLayout();
            this.pnlCardLowStock.ResumeLayout(false);
            this.pnlCardLowStock.PerformLayout();
            this.pnlCardActive.ResumeLayout(false);
            this.pnlCardActive.PerformLayout();
            this.pnlCardTotal.ResumeLayout(false);
            this.pnlCardTotal.PerformLayout();
            this.pnlFilterBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnNewProduct;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chkDarkMode;
        private System.Windows.Forms.Panel pnlProductCard;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtPrice;
        private System.Windows.Forms.TextBox txtStock;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.TextBox txtImageUrl;
        private System.Windows.Forms.CheckBox chkIsActive;
        private System.Windows.Forms.TextBox txtProductId;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlFilterBar;
        private System.Windows.Forms.ComboBox cmbFilterCategory;
        private System.Windows.Forms.ComboBox cmbFilterStockStatus;
        private System.Windows.Forms.ComboBox cmbFilterActive;
        private System.Windows.Forms.Panel pnlSummaryCard;
        private System.Windows.Forms.Panel pnlCardTotal;
        private System.Windows.Forms.Label lblCardTotalTitle;
        private System.Windows.Forms.Label lblCardTotalValue;
        private System.Windows.Forms.Panel pnlCardActive;
        private System.Windows.Forms.Label lblCardActiveTitle;
        private System.Windows.Forms.Label lblCardActiveValue;
        private System.Windows.Forms.Panel pnlCardLowStock;
        private System.Windows.Forms.Label lblCardLowTitle;
        private System.Windows.Forms.Label lblCardLowValue;
        private System.Windows.Forms.Panel pnlCardPassive;
        private System.Windows.Forms.Label lblCardPassiveTitle;
        private System.Windows.Forms.Label lblCardPassiveValue;
        private System.Windows.Forms.Panel pnlGrid;
        private System.Windows.Forms.DataGridView dgvProducts;
    }
}
