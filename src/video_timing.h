#pragma once
#include <algorithm>
#include <array>
#include <cstdint>
#include <mutex>
#include <vector>

// Exact nearest-rank P95 over the last 500 delivered video send calls.
// Copy under lock; sorting happens only on the telemetry thread, outside the lock.
class VideoTiming {
    std::mutex mutex;
    std::array<double,500> samples{};
    size_t size=0, next=0;
    uint64_t total=0, overBudget=0;
    double sessionMax=0;
public:
    struct Stats { size_t count=0; double average=0,p95=0,maximum=0,sessionMax=0; uint64_t total=0,overBudget=0; };
    void add(double milliseconds,double budget) {
        std::lock_guard lock(mutex);
        samples[next]=milliseconds; next=(next+1)%samples.size();
        size=std::min(size+1,samples.size()); ++total;
        if(milliseconds>=budget)++overBudget;
        sessionMax=std::max(sessionMax,milliseconds);
    }
    Stats snapshot() {
        std::vector<double> values; Stats result;
        { std::lock_guard lock(mutex);
          values.assign(samples.begin(),samples.begin()+size);
          result.sessionMax=sessionMax;result.total=total;result.overBudget=overBudget; }
        result.count=values.size(); if(values.empty())return result;
        double sum=0;for(double value:values)sum+=value;
        std::sort(values.begin(),values.end());
        result.average=sum/values.size();
        result.p95=values[(95*values.size()+99)/100-1];
        result.maximum=values.back();return result;
    }
};
