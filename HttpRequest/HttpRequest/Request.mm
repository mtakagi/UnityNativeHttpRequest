//
//  Request.m
//  HttpRequest
//
//  Created by mtakagi on 2017/09/08.
//  Copyright © 2017年 mtakagi. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "HttpClient.h"

extern "C" {
    void * HttpRequest_Create(const char *);
    void HttpRequest_SetHeader(const void *, const char *, const char *);
    void HttpRequest_SetMethod(const void *, const char *);
    void HttpRequest_SetHttpBody(const void *, const uint8_t *, const int);
    void HttpRequest_SetTimeout(const void *, const double);
    void HttpRequest_Release(const void *);

    void * HttpClient_CreateWithRequest(const void *, void *, ResponseCallback, DownloadPorgressCallback,
                              RequestFailedCallback,RequestFinishedCallback);
    void * HttpClient_Create(const char *, void *, ResponseCallback, DownloadPorgressCallback,
                              RequestFailedCallback,RequestFinishedCallback);
    void HttpClient_Start(void *);
    void HttpClient_Cancel(void *);
    void HttpClient_Dispose(void *);
}

void * HttpRequest_Create(const char * url) {
    NSURL *urlObj = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:urlObj];
    
    return (void *)request;
}

void HttpRequest_SetHeader(const void * request, const char * key, const char * value) {
    NSString * keyString = [NSString stringWithUTF8String:key];
    NSString * valueString = [NSString stringWithUTF8String:value];
    [(NSMutableURLRequest *)request addValue:valueString forHTTPHeaderField:keyString];
}

void HttpRequest_SetMethod(const void * request, const char * method) {
    NSString *methodString = [NSString stringWithUTF8String:method];
    ((NSMutableURLRequest *)request).HTTPMethod = methodString;
}

void HttpRequest_SetHttpBody(const void * request, const uint8_t *body, const int length) {
    NSData *data = [NSData dataWithBytes:body length:length];
    ((NSMutableURLRequest *)request).HTTPBody = data;
}

void HttpRequest_SetTimeout(const void * request, const double timeout) {
    ((NSMutableURLRequest *)request).timeoutInterval = timeout;
}

void HttpRequest_Release(const void * request) {
    [(NSMutableURLRequest *)request release];
}

void * HttpClient_CreateWithRequest(const void * mutableRequest, void *context,
                                     ResponseCallback responseCallback,
                                     DownloadPorgressCallback downloadProgressCallback,
                                     RequestFailedCallback requestFailedCallback,
                                     RequestFinishedCallback requestFinishedCallback) {
    HttpClient *client = [[HttpClient alloc] initWithRequest:(NSMutableURLRequest *)mutableRequest];
    client.context = context;
    client.responseCallback = responseCallback;
    client.downloadProgressCallback = downloadProgressCallback;
    client.requestFailedCallback = requestFailedCallback;
    client.requestFinishedCallback = requestFinishedCallback;
    
    return (void *)client;
}

void * HttpClient_Create(const char * url, void *context,
                          ResponseCallback responseCallback,
                          DownloadPorgressCallback downloadProgressCallback,
                          RequestFailedCallback requestFailedCallback,
                          RequestFinishedCallback requestFinishedCallback) {
    HttpClient *client = [[HttpClient alloc] initWithURL:[NSString stringWithCString:url
                                                                               encoding:NSUTF8StringEncoding]];
    client.context = context;
    client.responseCallback = responseCallback;
    client.downloadProgressCallback = downloadProgressCallback;
    client.requestFailedCallback = requestFailedCallback;
    client.requestFinishedCallback = requestFinishedCallback;

    return (void *)client;
}

void HttpClient_Start(void * client) {
    [(HttpClient *)client start];
}

void HttpRequest_Cancel(void * client) {
    [(HttpClient *)client cancel];
}

void HttpClient_Dispose(void * client) {
    [(HttpClient *)client release];
}
