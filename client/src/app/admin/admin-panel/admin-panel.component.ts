import {Component, OnDestroy, OnInit} from '@angular/core';
import {User} from "../../_models/User";
import {AdminService} from "../../_services/admin.service";
import {Subscription} from "rxjs";

@Component({
  selector: 'app-admin-panel',
  templateUrl: './admin-panel.component.html',
  styleUrls: ['./admin-panel.component.css']
})
export class AdminPanelComponent implements OnInit {

  constructor() {
  }

  ngOnInit(): void {

  }

}
