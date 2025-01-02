export interface Message {
    id: number
    senderId: number
    senderUsername: string
    senderPhotoUrl: string
    recipientId: number
    recipientPhotoUrl: any
    recipientUsername: string
    content: string
    dateRead?: Date
    messageSent?: Date
  }
  