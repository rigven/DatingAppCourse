import { CanDeactivateFn } from '@angular/router';
import { MemberEditComponent } from '../_members/member-edit/member-edit.component';

export const preventUnsavedChangesGuard: CanDeactivateFn<MemberEditComponent> = (component) => {
  if (component.editForm?.dirty) {
    return confirm('Ar you sure you want to continue? Any unsaved changes will be lost');
  }
  return true;
};
