﻿using DiegoPiov.UserManagement;
using EH;
using EH.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable
namespace EH_WindowsFormsApp
{
    public partial class FrmUsers : Form
    {
        static readonly string stringConnection = $"Data Source=172.27.13.97:49161/xe;User Id=system;Password=oracle";
        readonly Database db = new(stringConnection);
        private bool _isUpdate = false;
        private User? _userSelected;

        public FrmUsers()
        {
            InitializeComponent();
        }

        private void FrmUsuario_Load(object sender, EventArgs e)
        {
            //PrepareDatabase(db);
            GetUsers();
            GetCareers();
            GetGroups();

            GridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            GridUsers.MultiSelect = false;
            GridUsers.RowHeadersVisible = false;

            BtnClear_Click(sender, e);
        }

        /// <summary>
        /// Creates all necessary tables in the database, if necessary.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool PrepareDatabase(Database db)
        {
            var eh = new EnttityHelper(db);
            if (!eh.DbContext.ValidateConnection()) { MessageBox.Show("Unable to establish a connection to the database!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; };
                   
            eh.CreateTableIfNotExist<User>();
            eh.CreateTableIfNotExist<Career>();
            eh.CreateTableIfNotExist<Group>();

            // USERS
            User user = new() { Id = "admin", Name = "Diêgo Piovezana", Login = "admin", Email = "diego.piov@abc.com" };
            eh.Insert(user, nameof(user.Id));

            // CAREERS
            Career career = new() { IdCareer = 1, Name = "Administrador", CareerLevel = 1, Active = true };
            eh.Insert(career, nameof(career.IdCareer));
            career = new Career() { Active = true, IdCareer = 2, Name = "Operacional", CareerLevel = 2 };
            eh.Insert(career, nameof(career.IdCareer));
            career = new() { Active = true, IdCareer = 3, Name = "Supervisor", CareerLevel = 3 };
            eh.Insert(career, nameof(career.IdCareer));
            career = new() { Active = true, IdCareer = 4, Name = "Líder", CareerLevel = 4 };
            eh.Insert(career, nameof(career.IdCareer));

            // GROUPS
            Group group = new() { IdGroup = 1, Name = "Administração", Active = true };
            eh.Insert(group, nameof(group.IdGroup)); 
            group = new() { IdGroup = 2, Name = "Operação", Active = true };
            eh.Insert(group, nameof(group.IdGroup)); 
            group = new() { IdGroup = 3, Name = "Supervisão", Active = true };
            eh.Insert(group, nameof(group.IdGroup)); 
            group = new() { IdGroup = 4, Name = "Liderança", Active = true };
            eh.Insert(group, nameof(group.IdGroup));

            return true;
        }

        private void GetUsers()
        {
            //bs = new() { DataSource = UserDTO.SearchAllUser(false, db) };
            bs = new() { DataSource = new EnttityHelper(db).Get<User>().Where(u => !false || u.Active).ToList() };
            GridUsers.DataSource = bs;
            CbSupervisor.DataSource = null;

            if (bs?.List is null || bs.List.Count <= 0) return;
            CbSupervisor.DataSource = (IEnumerable<User>)bs.List;
            CbSupervisor.DisplayMember = "Id";
        }

        private void GetCareers()
        {
            var eh = new EnttityHelper(db);
            var careers = eh.Get<Career>().Where(c => c.Active).ToList();
            CbCareer.DataSource = careers;
            CbCareer.DisplayMember = nameof(Career.Name);
        }

        private void GetGroups()
        {
            var eh = new EnttityHelper(db);
            var groups = eh.Get<Group>().Where(c => c.Active).ToList();
            CbGroup.DataSource = groups;
            CbGroup.DisplayMember = nameof(Group.Name);
        }





        private void GridUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BtnClear_Click(sender, e);

            sbyte row = (sbyte)e.RowIndex;
            if (row >= 0)
            {
                FillData(e.RowIndex);
                BtnUpdate.Visible = true;
                BtnDelete.Visible = true;
            }
        }

        private void FillData(int index)
        {
            _userSelected = (User)GridUsers.Rows[index].DataBoundItem;
            if (_userSelected is null) return;

            TxtEmail.Text = _userSelected.Email;
            TxtLogin.Text = _userSelected.Login;
            TxtName.Text = _userSelected.Name;

            CbSupervisor.SelectedIndex = CbSupervisor.FindStringExact(_userSelected.Supervisor?.Id);
            CbCareer.Text = _userSelected.Career?.Name;
            CbGroup.Text = _userSelected.Group?.Name;

            CkbActive.Checked = _userSelected.Active;
            CkbInternal.Checked = _userSelected.InternalUser == "Y";  
        }

        private bool FillUser(User? user)
        {
            if (user is null) return false;

            user.Id = _isUpdate ? _userSelected?.Id : TxtEmail.Text.Split('@')[0];
            user.Email = TxtEmail.Text;
            user.Login = TxtLogin.Text;
            user.Name = TxtName.Text;
            user.Career = (Career)CbCareer.SelectedItem;
            user.IdCareer = string.IsNullOrEmpty(CbCareer.Text) ? 0 : ((Career)CbCareer.SelectedItem).IdCareer;
            user.Career = string.IsNullOrEmpty(CbCareer.Text) ? null : (Career)CbCareer.SelectedItem;
            user.IdGroup = string.IsNullOrEmpty(CbGroup.Text) ? 0 : ((Group)CbGroup.SelectedItem).IdGroup;
            user.Group = string.IsNullOrEmpty(CbGroup.Text) ? null : (Group)CbGroup.SelectedItem;
            user.IdSupervisor = string.IsNullOrEmpty(CbSupervisor.Text) ? null : ((User)CbSupervisor.SelectedItem).Id;
            user.Supervisor = string.IsNullOrEmpty(CbSupervisor.Text) ? null : (User)CbSupervisor.SelectedItem;
            user.InternalUser = CkbInternal.Checked ? "Y" : "N";

            if (user.Active && !CkbActive.Checked) user.DtDeactivation = DateTime.Now;
            if (!user.Active && CkbActive.Checked) user.DtActivation = DateTime.Now;

            user.Active = CkbActive.Checked;

            if (!_isUpdate) user.DtCreation = DateTime.Now;
            else user.DtAlteration = DateTime.Now;

            return true;
        }

        static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress mailAddress = new(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            TxtEmail.Text = "";
            TxtLogin.Text = "";
            TxtName.Text = "";   
            CbCareer.SelectedIndex = -1;
            CbGroup.SelectedIndex = -1;
            CbSupervisor.SelectedIndex = -1;

            CkbActive.Checked = false;
            CkbInternal.Checked = false;

            TxtEmail.Enabled = false;
            TxtLogin.Enabled = false;
            TxtName.Enabled = false;
            CbCareer.Enabled = false;
            CbGroup.Enabled = false;
            CbSupervisor.Enabled = false;
            CkbActive.Enabled = false;
            CkbInternal.Enabled = false;

            BtnUpdate.Visible = false;
            BtnDelete.Visible = false;
            BtnAdd.Visible = true;

            BtnSave.Visible = false;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            _isUpdate = true;

            TxtEmail.Enabled = false;
            TxtLogin.Enabled = true;
            TxtName.Enabled = true;
            CbCareer.Enabled = true;
            CbGroup.Enabled = true;
            CbSupervisor.Enabled = true;
            CkbActive.Enabled = true;
            CkbInternal.Enabled = true;

            BtnUpdate.Visible = false;
            BtnDelete.Visible = false;

            BtnSave.Visible = true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            BtnClear_Click(sender, e);

            _isUpdate = false;

            TxtEmail.Enabled = true;
            TxtLogin.Enabled = true;
            TxtName.Enabled = true;
            CbCareer.Enabled = true;
            CbGroup.Enabled = true;
            CbSupervisor.Enabled = true;
            CkbActive.Enabled = true;
            CkbActive.Checked = true;
            CkbInternal.Enabled = true;
            CkbInternal.Checked = true;

            BtnUpdate.Visible = false;
            BtnDelete.Visible = false;

            BtnSave.Visible = true;
        }

        private void BtnSalvar_Click(object sender, EventArgs e)
        {
            if (SaveUser())
            {
                BtnSave.Visible = false;
                GetUsers();
                BtnClear_Click(sender, e);
            }
        }

        private bool SaveUser()
        {
            try
            {
                var eh = new EnttityHelper(db);
                User user = new();

                if (_isUpdate)
                {
                    if (_userSelected is null) return false;
                    user = (User)_userSelected.Clone();
                }

                if (!FillUser(user)) return false;
                _userSelected = user;

                if (_isUpdate)
                {
                    if (eh.Update(user) > 0)
                    {
                        MessageBox.Show($"User '{user?.Id}' updated successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"User '{user.Id}' was not updated!", "Failed to save!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    switch (eh.Insert(user, nameof(user.Id)))
                    {
                        case int n when n > 0:
                            MessageBox.Show($"User '{user?.Id}' saved successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        case -101:
                            MessageBox.Show($"User '{user.Id}' already exists!", "User already exists!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        case 0:
                            MessageBox.Show($"User '{user.Id}' was not saved!", "Failed to save!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        default:
                            throw new Exception("Failed to register user!");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void TxtEmail_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtEmail.Text))
            {
                e.Cancel = true;
                TxtEmail.Focus();
                MessageBox.Show("The 'Email' field cannot be left blank!", "Email address not provided!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!IsValidEmail(TxtEmail.Text))
            {
                e.Cancel = true;
                TxtEmail.Focus();
                MessageBox.Show($"'{TxtEmail.Text}' is not a valid email!", "Invalid email address!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var eh = new EnttityHelper(db);

            if (_userSelected is not null && eh.Delete(_userSelected) > 0)
            {
                BtnClear_Click(sender, e);
                GetUsers();
                MessageBox.Show($"User '{_userSelected?.Id}' successfully deleted!", "User deleted!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"User '{_userSelected?.Id}' has not been deleted!", "Failed to delete!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}