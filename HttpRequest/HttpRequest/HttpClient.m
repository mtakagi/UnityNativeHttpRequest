//
//  HttpRequest.m
//  HttpRequest
//
//  Created by mtakagi on 2017/09/07.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import "HttpClient.h"

@implementation HttpClient
{
    BOOL _isCanceled;
}

static NSOperationQueue * _gQueue;

+ (void)initialize
{
    _gQueue = [[NSOperationQueue alloc] init];
    _gQueue.maxConcurrentOperationCount = [[NSProcessInfo processInfo] processorCount] * 5;
    _gQueue.name = @"com.0xabadbabe.HttpRequest";
}

- (instancetype)initWithRequest:(NSURLRequest *)request
{
    self = [super init];
    if (self) {
        _connection = [[NSURLConnection alloc] initWithRequest:request delegate:self];
        [_connection setDelegateQueue:_gQueue];
        _isCanceled = NO;
    }
    
    return self;
}

- (instancetype)initWithURL:(NSString *)url
{
    return [self initWithRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:url]]];
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
    _isCanceled = YES;
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
    if (self.downloadProgressCallback && !_isCanceled) {
        self.downloadProgressCallback(self.context, [data bytes], [data length]);
    }
}

- (void)connection:(NSURLConnection *)connection
didReceiveResponse:(NSURLResponse *)response
{
    if (self.responseCallback && !_isCanceled) {
        NSHTTPURLResponse *httpResponse = (NSHTTPURLResponse *)response;
        NSInteger statusCode = httpResponse.statusCode;
        NSDictionary *headers = httpResponse.allHeaderFields;
        NSString *statusString = [NSHTTPURLResponse localizedStringForStatusCode:statusCode];
        const char *keys[headers.count];
        const char *values[headers.count];
        int i = 0;
        
        for (NSString *key in headers) {
            NSString *value = [headers objectForKey:key];
            keys[i] = [key UTF8String];
            values[i] = [value UTF8String];
            i++;
        }
        self.responseCallback(self.context, statusCode, [statusString UTF8String], (int)headers.count, keys, values);
    }
}

- (void)connectionDidFinishLoading:(NSURLConnection *)connection
{
    if (self.requestFinishedCallback) {
        self.requestFinishedCallback(self.context);
    }
}

@end
