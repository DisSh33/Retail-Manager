﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Caliburn.Micro;
using RMDesktopUI.Helpers;
using RMDesktopUI.Library.Api;
using RMDesktopUI.Library.Models;
using RMDesktopUI.Models;

namespace RMDesktopUI.ViewModels
{
    public class UserDisplayViewModel : Screen
    {
        private readonly IDialogWindowHelper _dialogWindow;

        private readonly IUserEndpoint _userEndpoint;
        private readonly IMapper _mapper;

        private BindingList<UserDisplayModel> _users;
        private BindingList<string> _usersRoles;
        private BindingList<string> _availableRoles = new BindingList<string>();

        private UserDisplayModel _selectedUser;
        private string _selectedUserName;
        
        private string _selectedUsersRoleToRemove;
        private string _selectedAvailableRoleToAdd;

        public UserDisplayViewModel(IMapper mapper, IUserEndpoint userEndpoint, IDialogWindowHelper dialogWindow)
        {
            _dialogWindow = dialogWindow;
            _mapper = mapper;
            _userEndpoint = userEndpoint;
        }

        protected override async void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            try
            {
                await LoadUsers();
            }
            catch (Exception ex)
            {
                await _dialogWindow.ShowSystemError(ex, "User Management page");
                
                await TryCloseAsync();
            }
        }

        public BindingList<UserDisplayModel> Users
        {
            get => _users;
            set
            {
                _users = value;
                NotifyOfPropertyChange(() => Users);
            }
        }

        public UserDisplayModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                SelectedUserName = value.UserName;
                UsersRoles = new BindingList<string>(value.Roles.Select(x => x.Value).ToList());
                _ = LoadRoles();        //This needs to be awaited 

                NotifyOfPropertyChange(() => SelectedUser);
            }
        }
        public string SelectedUsersRoleToRemove
        {
            get => _selectedUsersRoleToRemove;
            set
            {
                _selectedUsersRoleToRemove = value;
                NotifyOfPropertyChange(() => SelectedUsersRoleToRemove);
                NotifyOfPropertyChange(() => Users);
                NotifyOfPropertyChange(() => CanRemoveSelectedRole);
            }
        }
        public string SelectedAvailableRoleToAdd
        {
            get => _selectedAvailableRoleToAdd;
            set
            {
                _selectedAvailableRoleToAdd = value;
                NotifyOfPropertyChange(() => SelectedAvailableRoleToAdd);
                NotifyOfPropertyChange(() => Users);
                NotifyOfPropertyChange(() => CanAddSelectedRole);
            }
        }
        public string SelectedUserName
        {
            get => _selectedUserName;
            set
            {
                _selectedUserName = value;
                NotifyOfPropertyChange(() => SelectedUserName);
            }
        }
        public BindingList<string> UsersRoles
        {
            get => _usersRoles;
            set
            {
                _usersRoles = value;
                NotifyOfPropertyChange(() => UsersRoles);
                NotifyOfPropertyChange(() => SelectedUser);
            }
        }
        public BindingList<string> AvailableRoles
        {
            get => _availableRoles;
            set
            {
                _availableRoles = value;
                NotifyOfPropertyChange(() => AvailableRoles);
            }
        }

        public async Task LoadUsers()
        {
            var userList = await _userEndpoint.GetAll();
            var users = _mapper.Map<List<UserDisplayModel>>(userList);
            Users = new BindingList<UserDisplayModel>(users);
        }

        private async Task LoadRoles()
        {
            var roles = await _userEndpoint.GetAllRoles();
            _availableRoles.Clear();

            foreach (var role in roles)
            {
                if (!UsersRoles.Contains(role.Value))
                {
                    _availableRoles.Add(role.Value);
                }
            }
        }

        public bool CanAddSelectedRole
        {
            get
            {
                var output = SelectedAvailableRoleToAdd != null;
                return output;
            }
        }
        public async void AddSelectedRole()
        {
            UserRolePairModel pairing = new UserRolePairModel(SelectedUser.Id, SelectedAvailableRoleToAdd);
            string role = SelectedAvailableRoleToAdd;

            await _userEndpoint.AddUserToRole(pairing);

            UsersRoles.Add(role);
            AvailableRoles.Remove(role);

            SelectedUser.RoleList = string.Join(", ", UsersRoles);
        }

        public bool CanRemoveSelectedRole
        {
            get
            {
                var output = SelectedUsersRoleToRemove != null;
                return output;
            }
        }
        public async void RemoveSelectedRole()
        {
            UserRolePairModel pairing = new UserRolePairModel(SelectedUser.Id, SelectedUsersRoleToRemove);
            string role = SelectedUsersRoleToRemove;

            await _userEndpoint.RemoveUserFromRole(pairing);

            var r = UsersRoles.Remove(role);
            var ur  = UsersRoles;
            AvailableRoles.Add(role);

            SelectedUser.RoleList = string.Join(", ", UsersRoles);
        }
    }
}
