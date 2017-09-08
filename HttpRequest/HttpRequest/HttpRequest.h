//
//  HttpRequest.h
//  HttpRequest
//
//  Created by mtakagi on 2017/09/08.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef void (*ResponseCallback)(void *);
typedef void (*DownloadPorgressCallback)(void *, const void *, const NSUInteger length);
typedef void (*RequestFailedCallback)(void *);
typedef void (*RequestFinishedCallback)(void *);

@interface HttpRequest : NSObject
{
    NSURLConnection * _connection;
}

- (instancetype)initWithURL:(NSString *)url;
- (void)start;
- (void)cancel;

@property (assign) void *responseContext;
@property (assign) void *downloadContext;
@property (assign) void *requestFailedContext;
@property (assign) void *requestFinishedContext;
@property (assign) ResponseCallback responseCallback;
@property (assign) DownloadPorgressCallback downloadProgressCallback;
@property (assign) RequestFailedCallback requestFailedCallback;
@property (assign) RequestFinishedCallback requestFinishedCallback;

@end

