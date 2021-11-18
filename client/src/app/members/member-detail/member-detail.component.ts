import {AfterViewChecked, AfterViewInit, Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {Member} from "../../_models/Member";
import {MembersService} from "../../_services/members.service";
import {ActivatedRoute, Router} from "@angular/router";
import {NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions} from "@kolkov/ngx-gallery";
import {TabDirective, TabsetComponent} from "ngx-bootstrap/tabs";
import {Message} from "../../_models/Message";
import {MessageService} from "../../_services/message.service";
import {PresenceService} from "../../_services/presence.service";
import {AccountService} from "../../_services/account.service";
import {User} from "../../_models/User";
import {take} from "rxjs/operators";

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activeTab: TabDirective;
  messages: Message[] = [];
  user: User;

  constructor(public presence: PresenceService, private route: ActivatedRoute, private router: Router
    , private messageService: MessageService, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
  }

  ngOnInit(): void {
    // this.loadMember();

    this.route.data.subscribe(data => {
      this.member = data.member;
    });

    this.route.queryParams.subscribe(params => {
      if (params.tab) {
        this.showTab(params.tab);
      }
    });

    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }];

    this.galleryImages = this.getImages();
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const image of this.member.photos) {
      imageUrls.push({
        small: image?.url,
        medium: image?.url,
        big: image?.url
      })
    }
    return imageUrls;
  }

  // loadMember() {
  //   //this.memberService.getMember(this.route.snapshot.paramMap.get('username'));
  //   this.memberService.getMember(this.route.snapshot.params.username).subscribe(user => {
  //     this.member = user;
  //     this.galleryImages = this.getImages();
  //   });
  // }

  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe(m => {
      this.messages = m;
    });
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === "Messages" && this.messages.length === 0) {
      //this.loadMessages();
      this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  showTab(index: number) {
    this.memberTabs.tabs[index].active = true;
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
