import { map } from 'rxjs/operators';
import { take } from 'rxjs/operators';
import { BsModalService } from 'ngx-bootstrap/modal';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Injectable } from '@angular/core';
import { ConfirmDialogComponent } from '../_modals/confirm-dialog/confirm-dialog.component';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModleRef: BsModalRef;

  constructor(private modalService: BsModalService) { }
  cofirm(title = 'Confirm', message = 'are you sure?', btnOkText = 'Ok', btnCancelTest = 'Cancel'): Observable<boolean> {
    const config = {
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelTest
      }
    };
    this.bsModleRef = this.modalService.show(ConfirmDialogComponent, config);
    return this.bsModleRef.onHidden.pipe(map(() => this.bsModleRef.content.result));
    // return new Observable<boolean>(oberver => {
    //   this.bsModleRef.onHidden.pipe(take(1)).subscribe(() => {
    //     oberver.next(this.bsModleRef.content.result);
    //   })
    // });
  }
}
