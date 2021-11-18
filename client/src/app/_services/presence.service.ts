import {Injectable} from '@angular/core';
import {environment} from "../../environments/environment";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {ToastrService} from "ngx-toastr";
import {User} from "../_models/User";
import {BehaviorSubject} from "rxjs";
import {take} from "rxjs/operators";
import {Router} from "@angular/router";

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsers = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsers.asObservable();

  constructor(private toastr: ToastrService, private router: Router) {
  }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    // this.hubConnection.on('UserIsOnline', username => {
    //   this.toastr.info(`${username} is online`)
    // });
    // this.hubConnection.on('UserIsOffline', username => {
    //   this.toastr.warning(`${username} is offline`)
    // });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsers.next(usernames);
    });

    this.hubConnection.on('NewMessageReceived', ({username, KnownAs}) => {
      this.toastr.info(`${KnownAs} sent a message`)
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl(`/members/${username}?tab=3`));
    });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
