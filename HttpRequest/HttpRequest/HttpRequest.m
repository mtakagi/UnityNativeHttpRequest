//
//  HttpRequest.m
//  HttpRequest
//
//  Created by mtakagi on 2017/09/07.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import "HttpRequest.h"

@implementation HttpRequest

static NSOperationQueue * _gQueue;

+ (void)initialize
{
    _gQueue = [[NSOperationQueue alloc] init];
    _gQueue.maxConcurrentOperationCount = [[NSProcessInfo processInfo] processorCount] * 5;
}

- (instancetype)initWithURL:(NSString *)url
{
    self = [super init];
    if (self) {
        NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL URLWithString:url]];
        _connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];
        [_connection setDelegateQueue:_gQueue];
    }
    
    return self;
}

- (void)dealloc {
    [_connection release];
    [super dealloc];
}

- (void)start
{
    [_connection start];
}

- (void)cancel
{
    [_connection cancel];
}

#pragma mark - Delegate

//- (void)connection:(NSURLConnection *)connection
//willSendRequestForAuthenticationChallenge:(NSURLAuthenticationChallenge *)challenge
//{
//    
//}

- (void)connection:(NSURLConnection *)connection
  didFailWithError:(NSError *)error
{
    
}

#pragma mark - Data Delegate

// キャッシュ無効
- (NSCachedURLResponse *)connection:(NSURLConnection *)connection
                  willCacheResponse:(NSCachedURLResponse *)cachedResponse
{
    return nil;
}

- (void)connection:(NSURLConnection *)connection
    didReceiveData:(NSData *)data
{
    if (self.downloadProgressCallback) {
        self.downloadProgressCallback(self.downloadContext, [data bytes], [data length]);
    }
}

- (void)connection:(NSURLConnection *)connection
didReceiveResponse:(NSURLResponse *)response
{
    
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection
{
    if (self.requestFinishedCallback) {
        self.requestFinishedCallback(self.requestFinishedContext);
    }
}

@end
