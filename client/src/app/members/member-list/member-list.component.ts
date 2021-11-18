import {Observable} from 'rxjs';
import {Component, OnInit} from '@angular/core';
import {Member} from '../../_models/Member';
import {MembersService} from '../../_services/members.service';
import {Pagination} from "../../_models/Pagination";
import {reflectObjectLiteral} from "@angular/compiler-cli/src/ngtsc/reflection";
import {UserParams} from "../../_models/userParams";
import {AccountService} from "../../_services/account.service";
import {take} from "rxjs/operators";
import {User} from "../../_models/User";

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent implements OnInit {
  // members$: Observable<Member[]>;
  members: Member[];
  pagination: Pagination;
  userParams: UserParams;
  user: User;
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  constructor(private memberService: MembersService) {
    this.userParams = this.memberService.getUserParams();
  }

  ngOnInit(): void {
    //this.members$ = this.memberService.getMembers();
    this.loadMembers();
  }

  // loadMembers() {
  //   this.memberService.getMembers().subscribe(members => {
  //     this.members = members;
  //   });
  // }

  loadMembers() {
    this.memberService.setUserParams(this.userParams);
    this.memberService.getMembers(this.userParams).subscribe(response => {
      this.members = response.Result;
      this.pagination = response.Pagination;
    });
  }

  pageChanged(e) {
    this.userParams.pageNumber = e.page;
    this.memberService.setUserParams(this.userParams);
    this.loadMembers();
  }

  resetFilters() {
    //this.userParams = new UserParams(this.user);
    this.userParams =this.memberService.resetUserParams(this.userParams);
    this.loadMembers();
  }
}
