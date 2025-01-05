export interface Message {
    id: number
    senderId: number
    senderUserName: string
    senderPhotoUrl: string
    recipientId: number
    recipientPhotoUrl: any
    recipientUserName: string
    content: string
    dateRead?: Date
    messageSent?: Date
  }
  