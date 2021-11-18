import {Component, HostListener, OnInit, ViewChild} from '@angular/core';
import {Member} from '../../_models/Member';
import {User} from '../../_models/User';
import {MembersService} from '../../_services/members.service';
import {AccountService} from '../../_services/account.service';
import {take} from 'rxjs/operators';
import {ToastrService} from 'ngx-toastr';
import {NgForm} from '@angular/forms';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css'],
})
export class MemberEditComponent implements OnInit {
  member: Member;
  user: User;
  @ViewChild('editForm') editForm: NgForm;

  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private memberService: MembersService,
    private accountService: AccountService,
    private toastr: ToastrService
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user) => {
      this.user = user;
      console.log(this.user.userName);
    });
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService
      .getMember(this.user.userName)
      .subscribe((member) => (this.member = member));
  }

  updateMember() {
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastr.success('profile updated successfully');
      this.editForm.reset(this.member);
    });
  }
}
