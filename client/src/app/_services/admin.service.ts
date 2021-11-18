import {Injectable} from '@angular/core';
import {environment} from "../../environments/environment";
import {HttpClient} from "@angular/common/http";
import {User} from "../_models/User";

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {
  }

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>(`${this.baseUrl}admin/user-with-roles`)
  }

  updateUserRoles(username: string, roles: string) {
    return this.http.post(`${this.baseUrl}admin/edit-role/${username}?roles=${roles}`, {});
  }
}
