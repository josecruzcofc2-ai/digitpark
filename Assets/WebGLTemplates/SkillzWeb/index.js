// unity tasks
const origin = '*';
const taskCallbacks = {};
let launchedSkillz = false;

function syncTask(topic, args) {
  switch (topic) {
    case 'GetMatch':
      return localStorage.getItem('match') || '';
    case 'GetPlayer':
      return localStorage.getItem('player');
    case 'GetRandom':
      return localStorage.getItem('random');
    case 'GetMatchToken':
      return localStorage.getItem('matchToken');
  }
}

function asyncTask(topic, args) {
  return new Promise((success, fail) => {
    // use case return if you don't need to wait for PortalPlay Response
    // use case break if you need to wait for PortalPlay Response
    switch (topic) {
      case 'SubmitScore':
        window.parent.postMessage(
          {
            topic,
            score: args,
          },
          origin
        );
        break;
      case 'GetMatch':
        const match = localStorage.getItem('match');
        success(match);
        return;
      case 'LaunchSkillz':
        window.parent.postMessage(
          {
            topic,
          },
          origin
        );
        launchedSkillz = true;
        success('success');
        return;
      case 'ReturnToSkillz':
        localStorage.removeItem('match');
        localStorage.removeItem('random');
        localStorage.removeItem('player');
        localStorage.removeItem('matchToken');
        window.parent.postMessage(
          {
            topic,
          },
          origin
        );
        // notifyUnity("OnSkillzWillExit", "")
        success('success');
        return;
      case 'UpdateScore':
        window.parent.postMessage(
          {
            topic,
            score: args,
          },
          origin
        );
        return;
      case 'GetProgressionUserData':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
        break;
      case 'UpdateProgressionUserData':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
        break;
      case 'GetCurrentSeason':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
        break;
      case 'GetPreviousSeasons':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
        break;
      case 'GetNextSeasons':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
        break;
      case 'AbortMatch':
        window.parent.postMessage(
          {
            topic,
            data: args,
          },
          origin
        );
    }
    taskCallbacks[`${topic}:callback`] = {
      success,
      fail,
    };
  });
}

function notifyUnity(event, data) {
  if (!myUnityInstance) {
    console.debug('myUnityInstance not found, eg. Failed to download file *.data.br');
  } else {
    myUnityInstance.SendMessage('SkillzDelegate', event, data);
    console.debug('notifyUnity', event, data);
  }
}

//listen to messages from the parent
window.addEventListener('message', (ev) => {
  const {topic, payload, error} = ev.data;
  if (!topic) return;
  console.log({topic, payload, error});
  switch (topic) {
    case 'OnMatchWillBegin':
      const {match, random, player, matchToken} = payload;
      console.log('event: OnMatchWillBegin');
      localStorage.setItem('match', JSON.stringify(match));
      localStorage.setItem('random', JSON.stringify(random));
      localStorage.setItem('player', JSON.stringify(player));
      localStorage.setItem('matchToken', matchToken);
      notifyUnity('OnMatchWillBegin', JSON.stringify(match));
      break;
    case 'OnTutorialScreenEnter':
      notifyUnity('OnTutorialScreenEnter');
      break;
    case 'IsSkillzLaunched':
      if (launchedSkillz) {
        window.parent.postMessage({topic: 'LaunchSkillz'}, origin);
      }
      break;
    case 'OnProgressionRoomEnter':
      notifyUnity('OnProgressionRoomEnter');
      break;
    case 'OnReceivedMemoryWarning':
      notifyUnity('OnReceivedMemoryWarning');
      break;
    case 'ResetGame':
      cleanUp();
      break;
    default:
      if (!taskCallbacks[topic]) {
        console.log('taskCallbacks not found:', topic);
        return;
      }
      if (error) {
        console.log('taskCallbacks got error, call fail:', error);
        taskCallbacks[topic].fail(error);
      } else {
        console.log('taskCallbacks got payload, call success:', payload);
        taskCallbacks[topic].success(payload);
      }
      delete taskCallbacks[topic];
  }
});

if ('serviceWorker' in navigator) {
  console.log('Service worker available');
  navigator.serviceWorker
    .register('/service-worker.js', {
      scope: '/',
    })
    .then((registration) => {
      console.log('Service Worker registered with scope:', registration.scope);
    })
    .catch((error) => {
      console.log('Service Worker registration failed:', error);
    });
} else {
  console.log('Service worker not available');
}
