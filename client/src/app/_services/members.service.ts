import {map, take} from 'rxjs/operators';
import {Injectable} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpClient, HttpHeaders, HttpParams, HttpResponse} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {Member} from '../_models/Member';
import {PaginatedResult} from "../_models/Pagination";
import {UserParams} from "../_models/userParams";
import {AccountService} from "./account.service";
import {User} from "../_models/User";
import {getPaginatedResult, LoadPaginationParams} from "./paginationHelpers";

// const options = {
//   headers: new HttpHeaders({
//     Authorization: 'Bearer ' + JSON.parse(localStorage.getItem('user')).token
//   })
// }

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  PaginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();
  memberCache = new Map();
  user: User;
  userParams: UserParams;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    });
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams(params) {
    this.userParams = new UserParams(this.user);
    return this.userParams;
  }

  // getMembers() {
  //   if (this.members.length > 0) return of(this.members);
  //   return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
  //     map((members) => {
  //       this.members = members;
  //       return this.members;
  //     })
  //   );
  //   // return this.http.get<Member[]>(this.baseUrl + 'users', options);
  // }

  getMembers(userParams: UserParams) {
    let response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) return of(response);

    const params = this.LoadParams(userParams);
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(map(response => { //set the cache
      this.memberCache.set(Object.values(userParams).join('-'), response);
      return response;
    }));
  }

  getMember(username: string) {
    const member = this.members.find((x) => x.username === username);
    if (member !== undefined) return of(member);
    return this.http.get<Member>(this.baseUrl + `users/${username}`);
    // return this.http.get<Member[]>(this.baseUrl + `users/${username}`, options);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(`${environment.apiUrl}users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(`${environment.apiUrl}users/delete-photo/${photoId}`);
  }

  LoadParams(userParams: UserParams): HttpParams {
    let params = LoadPaginationParams(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
    return params;
  }

  // getPaginatedResult<T>(url: string, params: HttpParams) {
  //   const paginationResult: PaginatedResult<T> = new PaginatedResult<T>()
  //   return this.http.get<T>(url, {observe: 'response', params}).pipe(
  //     map(response => {
  //       paginationResult.Result = response.body;
  //       if (response.headers.get('Pagination') !== null) {
  //         paginationResult.Pagination = JSON.parse(response.headers.get('Pagination'));
  //       }
  //       return paginationResult;
  //     }));
  // }

  addLike(username: string) {
    return this.http.post(`${this.baseUrl}likes/${username}`, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = new HttpParams();
    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());
    params = params.append('predicate', predicate);
    //return this.http.get<Partial<Member[]>>(`${this.baseUrl}likes`, {params});
    return getPaginatedResult<Partial<Member[]>>(`${this.baseUrl}likes`, params, this.http);
  }
}
