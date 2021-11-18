import { take } from 'rxjs/operators';
import { ConfirmService } from './../_services/confirm.service';
import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MemberEditComponent } from "../members/member-edit/member-edit.component";

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<unknown> {
  constructor(private cnofirmService: ConfirmService) { }
  canDeactivate(component: MemberEditComponent): Observable<boolean> | boolean {
    if (component.editForm.dirty) {
      // return confirm('sure you want to leave any, any changes will be discarded');
      return this.cnofirmService.cofirm('Watch Out!!!', 'Changes won\'t be saved').pipe(take(1));
    }
    return true;
  }

}
