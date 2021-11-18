import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from "../../_models/Message";
import { MessageService } from "../../_services/message.service";
import { NgForm } from "@angular/forms";

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() messages: Message[];
  @Input() username: string;
  @ViewChild('messageForm') messageForm: NgForm;
  content: string;

  constructor(public messageService: MessageService) {
  }

  ngOnInit(): void {
  }

  sendMessage() {
    // this.messageService.sendMessage(this.username, this.content)
    //   .subscribe(message => this.messages.push(message));
    this.messageService.sendMessage(this.username, this.content).then(() => this.messageForm.reset());
  }

}
