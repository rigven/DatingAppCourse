import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { AccountService } from '../../_services/account.service';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [TabsModule, FormsModule],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit{
  @ViewChild('editForm') editForm?: NgForm;
  //For preveting unsaved changes by browser when closing the website tab.
  @HostListener('window:beforeunload', ['$event']) notify($event: any){
    if (this.editForm?.dirty) {
      $event.returnValue = true
    }
  }
  member?: Member;
  private membersService = inject(MembersService);
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService)

  ngOnInit() {
    this.loadMember();
  }

  loadMember() {
    const user = this.accountService.currentUser();
    if (!user) return;
    this.membersService.getMember(user.userName).subscribe({
      next: member => this.member = member
    })
  }

  updateMember() {
    this.membersService.updateMember(this.editForm?.value).subscribe({
      next: _ => {
        this.toastr.success("Profile updated successfully");
        this.editForm?.reset(this.member);
      }
    })
    
  }
}
