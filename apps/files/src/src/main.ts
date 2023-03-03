import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);
  await app.listen(process.env.PORT || 80, '0.0.0.0');
  console.log(`service running at ${await app.getUrl()}`);
}
bootstrap();