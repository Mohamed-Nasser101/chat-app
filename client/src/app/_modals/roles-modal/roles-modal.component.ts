import {Component, EventEmitter, Input, OnInit} from '@angular/core';
import {BsModalRef} from "ngx-bootstrap/modal";
import {User} from "../../_models/User";

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  @Input() updateSelectedRoles = new EventEmitter();
  user: User;
  roles: { name: string, value: string, checked: boolean }[];

  constructor(public bsModalRef: BsModalRef) {
  }

  ngOnInit(): void {
  }

  updateRoles() {
    this.updateSelectedRoles.emit(this.roles);
    this.bsModalRef.hide();
  }

  hasRole(): boolean {
    return this.roles.filter(x => x.checked).length > 0;
  }
}
