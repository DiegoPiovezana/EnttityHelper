namespace EH_WindowsFormsApp
{
    partial class FrmUsers
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmUsers));
            this.LblEmail = new System.Windows.Forms.Label();
            this.TxtEmail = new System.Windows.Forms.TextBox();
            this.LblName = new System.Windows.Forms.Label();
            this.TxtName = new System.Windows.Forms.TextBox();
            this.Supervisor = new System.Windows.Forms.Label();
            this.CbSupervisor = new System.Windows.Forms.ComboBox();
            this.CkbActive = new System.Windows.Forms.CheckBox();
            this.CkbInternal = new System.Windows.Forms.CheckBox();
            this.BtnSave = new System.Windows.Forms.Button();
            this.PanelUsuario = new System.Windows.Forms.Panel();
            this.CbGroup = new System.Windows.Forms.ComboBox();
            this.CbCareer = new System.Windows.Forms.ComboBox();
            this.lblLogin = new System.Windows.Forms.Label();
            this.TxtLogin = new System.Windows.Forms.TextBox();
            this.LblGroup = new System.Windows.Forms.Label();
            this.LblCarrier = new System.Windows.Forms.Label();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BtnUpdate = new System.Windows.Forms.Button();
            this.BtnClear = new System.Windows.Forms.Button();
            this.GridUsers = new ADGV.AdvancedDataGridView();
            this.bs = new System.Windows.Forms.BindingSource(this.components);
            this.BtnDelete = new System.Windows.Forms.Button();
            this.TxtSearch = new System.Windows.Forms.TextBox();
            this.CklbUsers = new System.Windows.Forms.CheckedListBox();
            this.PanelUsuario.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs)).BeginInit();
            this.SuspendLayout();
            // 
            // LblEmail
            // 
            this.LblEmail.AutoSize = true;
            this.LblEmail.Location = new System.Drawing.Point(14, 12);
            this.LblEmail.Name = "LblEmail";
            this.LblEmail.Size = new System.Drawing.Size(46, 16);
            this.LblEmail.TabIndex = 1;
            this.LblEmail.Text = "Email*";
            // 
            // TxtEmail
            // 
            this.TxtEmail.Location = new System.Drawing.Point(17, 32);
            this.TxtEmail.Name = "TxtEmail";
            this.TxtEmail.Size = new System.Drawing.Size(204, 22);
            this.TxtEmail.TabIndex = 1;
            this.TxtEmail.Validating += new System.ComponentModel.CancelEventHandler(this.TxtEmail_Validating);
            // 
            // LblName
            // 
            this.LblName.AutoSize = true;
            this.LblName.Location = new System.Drawing.Point(13, 140);
            this.LblName.Name = "LblName";
            this.LblName.Size = new System.Drawing.Size(49, 16);
            this.LblName.TabIndex = 1;
            this.LblName.Text = "Name*";
            // 
            // TxtName
            // 
            this.TxtName.Location = new System.Drawing.Point(16, 160);
            this.TxtName.Name = "TxtName";
            this.TxtName.Size = new System.Drawing.Size(204, 22);
            this.TxtName.TabIndex = 3;
            // 
            // Supervisor
            // 
            this.Supervisor.AutoSize = true;
            this.Supervisor.Location = new System.Drawing.Point(14, 333);
            this.Supervisor.Name = "Supervisor";
            this.Supervisor.Size = new System.Drawing.Size(77, 16);
            this.Supervisor.TabIndex = 1;
            this.Supervisor.Text = "Supervisor*";
            // 
            // CbSupervisor
            // 
            this.CbSupervisor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbSupervisor.FormattingEnabled = true;
            this.CbSupervisor.Location = new System.Drawing.Point(17, 353);
            this.CbSupervisor.Name = "CbSupervisor";
            this.CbSupervisor.Size = new System.Drawing.Size(204, 24);
            this.CbSupervisor.TabIndex = 6;
            // 
            // CkbActive
            // 
            this.CkbActive.AutoSize = true;
            this.CkbActive.Location = new System.Drawing.Point(17, 398);
            this.CkbActive.Name = "CkbActive";
            this.CkbActive.Size = new System.Drawing.Size(66, 20);
            this.CkbActive.TabIndex = 7;
            this.CkbActive.Text = "Active";
            this.CkbActive.UseVisualStyleBackColor = true;
            // 
            // CkbInternal
            // 
            this.CkbInternal.AutoSize = true;
            this.CkbInternal.Location = new System.Drawing.Point(17, 426);
            this.CkbInternal.Name = "CkbInternal";
            this.CkbInternal.Size = new System.Drawing.Size(101, 20);
            this.CkbInternal.TabIndex = 8;
            this.CkbInternal.Text = "Internal user";
            this.CkbInternal.UseVisualStyleBackColor = true;
            // 
            // BtnSave
            // 
            this.BtnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnSave.Location = new System.Drawing.Point(70, 465);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(107, 39);
            this.BtnSave.TabIndex = 9;
            this.BtnSave.Text = "Save";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSalvar_Click);
            // 
            // PanelUsuario
            // 
            this.PanelUsuario.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelUsuario.BackColor = System.Drawing.Color.WhiteSmoke;
            this.PanelUsuario.Controls.Add(this.CbGroup);
            this.PanelUsuario.Controls.Add(this.CbCareer);
            this.PanelUsuario.Controls.Add(this.CbSupervisor);
            this.PanelUsuario.Controls.Add(this.BtnSave);
            this.PanelUsuario.Controls.Add(this.LblEmail);
            this.PanelUsuario.Controls.Add(this.CkbInternal);
            this.PanelUsuario.Controls.Add(this.TxtEmail);
            this.PanelUsuario.Controls.Add(this.CkbActive);
            this.PanelUsuario.Controls.Add(this.lblLogin);
            this.PanelUsuario.Controls.Add(this.TxtLogin);
            this.PanelUsuario.Controls.Add(this.LblName);
            this.PanelUsuario.Controls.Add(this.TxtName);
            this.PanelUsuario.Controls.Add(this.LblGroup);
            this.PanelUsuario.Controls.Add(this.LblCarrier);
            this.PanelUsuario.Controls.Add(this.Supervisor);
            this.PanelUsuario.Location = new System.Drawing.Point(1113, 48);
            this.PanelUsuario.Name = "PanelUsuario";
            this.PanelUsuario.Size = new System.Drawing.Size(241, 705);
            this.PanelUsuario.TabIndex = 6;
            // 
            // CbGroup
            // 
            this.CbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbGroup.FormattingEnabled = true;
            this.CbGroup.Location = new System.Drawing.Point(17, 288);
            this.CbGroup.Name = "CbGroup";
            this.CbGroup.Size = new System.Drawing.Size(204, 24);
            this.CbGroup.TabIndex = 5;
            // 
            // CbCareer
            // 
            this.CbCareer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbCareer.FormattingEnabled = true;
            this.CbCareer.Location = new System.Drawing.Point(16, 224);
            this.CbCareer.Name = "CbCareer";
            this.CbCareer.Size = new System.Drawing.Size(204, 24);
            this.CbCareer.TabIndex = 4;
            // 
            // lblLogin
            // 
            this.lblLogin.AutoSize = true;
            this.lblLogin.Location = new System.Drawing.Point(14, 76);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(45, 16);
            this.lblLogin.TabIndex = 1;
            this.lblLogin.Text = "Login*";
            // 
            // TxtLogin
            // 
            this.TxtLogin.Location = new System.Drawing.Point(17, 96);
            this.TxtLogin.Name = "TxtLogin";
            this.TxtLogin.Size = new System.Drawing.Size(204, 22);
            this.TxtLogin.TabIndex = 2;
            // 
            // LblGroup
            // 
            this.LblGroup.AutoSize = true;
            this.LblGroup.Location = new System.Drawing.Point(14, 268);
            this.LblGroup.Name = "LblGroup";
            this.LblGroup.Size = new System.Drawing.Size(49, 16);
            this.LblGroup.TabIndex = 1;
            this.LblGroup.Text = "Group*";
            // 
            // LblCarrier
            // 
            this.LblCarrier.AutoSize = true;
            this.LblCarrier.Location = new System.Drawing.Point(13, 204);
            this.LblCarrier.Name = "LblCarrier";
            this.LblCarrier.Size = new System.Drawing.Size(52, 16);
            this.LblCarrier.TabIndex = 1;
            this.LblCarrier.Text = "Carrier*";
            // 
            // BtnAdd
            // 
            this.BtnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAdd.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.BtnAdd.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnAdd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnAdd.Location = new System.Drawing.Point(1324, 12);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(30, 30);
            this.BtnAdd.TabIndex = 7;
            this.BtnAdd.Text = "➕";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // BtnUpdate
            // 
            this.BtnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnUpdate.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.BtnUpdate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Olive;
            this.BtnUpdate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.BtnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnUpdate.Location = new System.Drawing.Point(1250, 12);
            this.BtnUpdate.Name = "BtnUpdate";
            this.BtnUpdate.Size = new System.Drawing.Size(30, 30);
            this.BtnUpdate.TabIndex = 7;
            this.BtnUpdate.Text = "✏️";
            this.BtnUpdate.UseVisualStyleBackColor = true;
            this.BtnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);
            // 
            // BtnClear
            // 
            this.BtnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnClear.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.BtnClear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gold;
            this.BtnClear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.BtnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnClear.Location = new System.Drawing.Point(1113, 12);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(30, 30);
            this.BtnClear.TabIndex = 7;
            this.BtnClear.Text = "🧹";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // GridUsers
            // 
            this.GridUsers.AllowUserToAddRows = false;
            this.GridUsers.AllowUserToDeleteRows = false;
            this.GridUsers.AllowUserToOrderColumns = true;
            this.GridUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GridUsers.AutoGenerateContextFilters = true;
            this.GridUsers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.GridUsers.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.GridUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridUsers.DateWithTime = false;
            this.GridUsers.Location = new System.Drawing.Point(12, 48);
            this.GridUsers.Name = "GridUsers";
            this.GridUsers.RowHeadersWidth = 51;
            this.GridUsers.RowTemplate.Height = 24;
            this.GridUsers.Size = new System.Drawing.Size(829, 705);
            this.GridUsers.TabIndex = 8;
            this.GridUsers.TimeFilter = false;
            this.GridUsers.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.GridUsers_CellClick);
            // 
            // bs
            // 
            this.bs.AllowNew = false;
            // 
            // BtnDelete
            // 
            this.BtnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnDelete.FlatAppearance.BorderColor = System.Drawing.SystemColors.Control;
            this.BtnDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.BtnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.BtnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnDelete.Location = new System.Drawing.Point(1286, 12);
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Size = new System.Drawing.Size(30, 30);
            this.BtnDelete.TabIndex = 7;
            this.BtnDelete.Text = "🗑️";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // TxtSearch
            // 
            this.TxtSearch.Location = new System.Drawing.Point(12, 20);
            this.TxtSearch.Name = "TxtSearch";
            this.TxtSearch.Size = new System.Drawing.Size(669, 22);
            this.TxtSearch.TabIndex = 9;
            // 
            // CklbUsers
            // 
            this.CklbUsers.FormattingEnabled = true;
            this.CklbUsers.Location = new System.Drawing.Point(848, 48);
            this.CklbUsers.Name = "CklbUsers";
            this.CklbUsers.Size = new System.Drawing.Size(259, 701);
            this.CklbUsers.TabIndex = 10;
            // 
            // FrmUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1366, 765);
            this.Controls.Add(this.CklbUsers);
            this.Controls.Add(this.TxtSearch);
            this.Controls.Add(this.GridUsers);
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.BtnClear);
            this.Controls.Add(this.BtnUpdate);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.PanelUsuario);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmUsers";
            this.Text = "User management";
            this.Load += new System.EventHandler(this.FrmUsuario_Load);
            this.PanelUsuario.ResumeLayout(false);
            this.PanelUsuario.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label LblEmail;
        private System.Windows.Forms.TextBox TxtEmail;
        private System.Windows.Forms.Label LblName;
        private System.Windows.Forms.TextBox TxtName;
        private System.Windows.Forms.Label Supervisor;
        private System.Windows.Forms.ComboBox CbSupervisor;
        private System.Windows.Forms.CheckBox CkbActive;
        private System.Windows.Forms.CheckBox CkbInternal;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Panel PanelUsuario;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.Button BtnUpdate;
        private System.Windows.Forms.Button BtnClear;
        private ADGV.AdvancedDataGridView GridUsers;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.TextBox TxtLogin;
        private System.Windows.Forms.ComboBox CbGroup;
        private System.Windows.Forms.ComboBox CbCareer;
        private System.Windows.Forms.Label LblGroup;
        private System.Windows.Forms.Label LblCarrier;
        private System.Windows.Forms.Button BtnDelete;
        private System.Windows.Forms.TextBox TxtSearch;
        private System.Windows.Forms.CheckedListBox CklbUsers;
        private System.Windows.Forms.BindingSource bs;
    }
}

