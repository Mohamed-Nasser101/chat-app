import {Component, OnDestroy, OnInit} from '@angular/core';
import {User} from "../../_models/User";
import {Subscription} from "rxjs";
import {AdminService} from "../../_services/admin.service";
import {BsModalRef, BsModalService} from "ngx-bootstrap/modal";
import {RolesModalComponent} from "../../_modals/roles-modal/roles-modal.component";

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: User[];
  sub: Subscription;
  modalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) {
  }

  ngOnInit(): void {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.sub = this.adminService.getUsersWithRoles().subscribe(users => this.users = users);
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getAvailableRolesForUser(user),
      }
    }
    this.modalRef = this.modalService.show(RolesModalComponent, config);
    this.modalRef.content.updateSelectedRoles.subscribe(roles => {
      const rolesToUpdate = {
        roles: [...roles.filter(el => el.checked).map(el => el.name)]
      }
      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user.userName, rolesToUpdate.roles.join(',')).subscribe(() => {
          user.roles = [...rolesToUpdate.roles];
        });
      }
    });
  }

  private getAvailableRolesForUser(user: User) {
    let roles = [
      {name: "Admin", value: "Admin", checked: false},
      {name: "Moderator", value: "Moderator", checked: false},
      {name: "Member", value: "Member", checked: false}
    ];
    user.roles.forEach(value => {
      roles.find(x => x.name == value).checked = true;
    });
    return roles;
  }
}
