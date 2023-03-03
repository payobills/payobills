import { Injectable } from '@nestjs/common';

@Injectable()
export class AppService {
  getAbout(): { app: string } {
    return { app: 'files-service' };
  }
}
