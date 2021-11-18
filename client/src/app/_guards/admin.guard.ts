import {Injectable} from '@angular/core';
import {CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router} from '@angular/router';
import {Observable} from 'rxjs';
import {AccountService} from "../_services/account.service";
import {map, take} from "rxjs/operators";
import {ToastrService} from "ngx-toastr";

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private accountService: AccountService, private router: Router, private toastr: ToastrService) {
  }

  canActivate(): Observable<boolean | UrlTree> {
    return this.accountService.currentUser$.pipe(take(1), map(user => {
      if (user.roles.includes('Admin') || user.roles.includes('Moderator')) {
        return true;
      } else {
        this.toastr.error('you aren\'t an admin')
        return this.router.createUrlTree(['/members']);
      }
    }));
  }

}
