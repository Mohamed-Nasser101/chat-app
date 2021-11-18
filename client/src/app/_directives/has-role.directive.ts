import {Directive, Input, OnInit, TemplateRef, ViewContainerRef} from '@angular/core';
import {AccountService} from "../_services/account.service";
import {User} from "../_models/User";
import {take} from "rxjs/operators";

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  user: User;

  constructor(private accountService: AccountService, private temp: TemplateRef<any>,
              private view: ViewContainerRef) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    if (!this.user?.roles || this.user == null) {
      this.view.clear();
      return;
    }
    if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
      this.view.createEmbeddedView(this.temp);
    } else {
      this.view.clear();
    }
  }

}
