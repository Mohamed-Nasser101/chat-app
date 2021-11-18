import { ConfirmService } from './../_services/confirm.service';
import { Component, OnInit } from '@angular/core';
import { Message } from "../_models/Message";
import { Pagination } from "../_models/Pagination";
import { MessageService } from "../_services/message.service";
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loading = false;

  constructor(private messageService: MessageService, private confirmService: ConfirmService) {
  }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe(response => {
      this.messages = response.Result;
      this.pagination = response.Pagination;
      this.loading = false;
    });
  }

  pageChanged(e) {
    this.pageNumber = e.page;
    this.loadMessages();
  }

  // deleteMessage(id: number) {
  //   this.confirmService.cofirm().subscribe(result => {
  //     if (result) {
  //       this.messageService.deleteMessage(id)
  //         // .subscribe(() => this.messages.splice(this.messages.findIndex(x => x.id === id), 1));
  //         .subscribe(() => this.messages = this.messages.filter((m, i) => id !== m.id));
  //     }
  //   })
  // }
  deleteMessage(id: number) {
    this.confirmService.cofirm().pipe(
      switchMap(result => {
        if (result) {
          this.messages = this.messages.filter((m, i) => id !== m.id)
          return this.messageService.deleteMessage(id)
        }
        return of();
      })
    ).subscribe();
  }
}
