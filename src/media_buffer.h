#pragma once
#include <algorithm>
#include <cmath>
#include <cstdint>
#include <deque>
#include <mutex>
#include <vector>
#include <stdexcept>
struct VideoPicture { int64_t timestamp=0; std::vector<uint8_t> pixels; };
struct Sound { int64_t timestamp=0; int samples=0; std::vector<float> planar; };
inline int32_t pcm32(float x) {
    if(!std::isfinite(x))return 0;
    const double v=std::clamp(static_cast<double>(x),-1.0,1.0);
    return v>=1 ? INT32_MAX : static_cast<int32_t>(v*2147483648.0);
}
class MediaBuffer {
    std::mutex mutex;
    std::deque<VideoPicture> video;
    std::deque<Sound> audio;
    int64_t previous=-1;
    uint64_t generation=0,discarded=0;
public:
    struct State {size_t count;int64_t first,last;uint64_t generation,discarded;};
    void push(VideoPicture picture) {
        std::lock_guard lock(mutex);
        if(previous>=0 && (picture.timestamp<previous || picture.timestamp-previous>10000000)){
            video.clear();audio.clear();++generation;
        }
        if(picture.timestamp==previous)return;
        previous=picture.timestamp;
        if(video.size()>=12){video.pop_front();++discarded;}
        video.push_back(std::move(picture));
    }
    void push(Sound sound) {
        std::lock_guard lock(mutex);
        if(!audio.empty() && sound.timestamp<audio.back().timestamp)audio.clear();
        while(!audio.empty() && (audio.size()>=64 || sound.timestamp-audio.front().timestamp>5000000))audio.pop_front();
        audio.push_back(std::move(sound));
    }
    State state() { std::lock_guard lock(mutex);
        return {video.size(),video.empty()?0:video.front().timestamp,previous,generation,discarded}; }
    bool picture(int64_t target,int64_t tolerance,VideoPicture& selected) {
        std::lock_guard lock(mutex);bool found=false;
        while(!video.empty() && video.front().timestamp<=target+tolerance){
            if(found)++discarded;
            selected=std::move(video.front());video.pop_front();found=true;
        }return found;
    }
    // Timestamp-align stereo PCM to the same media interval as the scheduled picture.
    // OMT is planar float; DeckLink expects interleaved signed PCM32 at 48 kHz.
    std::vector<int32_t> sound(int64_t target,int count,uint64_t& missing) {
        std::lock_guard lock(mutex);std::vector<int32_t> result(static_cast<size_t>(count)*2,0);
        std::vector<bool> present(count,false);
        const int64_t end=target+static_cast<int64_t>(count)*10000000/48000;
        while(!audio.empty() && audio.front().timestamp+static_cast<int64_t>(audio.front().samples)*10000000/48000<=target)audio.pop_front();
        for(const auto& chunk:audio){
            if(chunk.timestamp>=end)break;
            const int64_t offset=std::llround(static_cast<double>(chunk.timestamp-target)*48000/10000000);
            for(int i=0;i<chunk.samples;++i){auto dest=offset+i;
                if(dest>=0 && dest<count){
                    result[static_cast<size_t>(dest)*2]=pcm32(chunk.planar[i]);
                    result[static_cast<size_t>(dest)*2+1]=pcm32(chunk.planar[chunk.samples+i]);
                    present[static_cast<size_t>(dest)]=true;
                }
            }
        }
        missing+=static_cast<uint64_t>(std::count(present.begin(),present.end(),false));return result;
    }
};
