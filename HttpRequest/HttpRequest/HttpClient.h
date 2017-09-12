//
//  HttpRequest.h
//  HttpRequest
//
//  Created by mtakagi on 2017/09/08.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef void (*ResponseCallback)(void *, NSInteger, const char *, const int,const char *keys[], const char *values[]);
typedef void (*DownloadPorgressCallback)(void *, const void *, const NSUInteger length);
typedef void (*RequestFailedCallback)(void *);
typedef void (*RequestFinishedCallback)(void *);

@interface HttpClient : NSObject
{
    NSURLConnection * _connection;
}

- (instancetype)initWithRequest:(NSURLRequest *)request;
- (instancetype)initWithURL:(NSString *)url;
- (void)start;
- (void)cancel;

@property (assign) void *context;
@property (assign) ResponseCallback responseCallback;
@property (assign) DownloadPorgressCallback downloadProgressCallback;
@property (assign) RequestFailedCallback requestFailedCallback;
@property (assign) RequestFinishedCallback requestFinishedCallback;

@end

