import { ref, type Ref } from 'vue'
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr'

interface RealtimeEvent {
  eventName: string
  payload: unknown
}

type EventListener = (event: RealtimeEvent) => void

class RealtimeClient {
  private connection: HubConnection | null = null
  private listeners = new Set<EventListener>()
  private accessTokenFactory: () => string | null = () => null

  /** reactive connection state — bind in UI for live indicators */
  public readonly state: Ref<HubConnectionState> = ref(HubConnectionState.Disconnected)

  configure(accessTokenFactory: () => string | null) {
    this.accessTokenFactory = accessTokenFactory
  }

  async connect() {
    if (this.connection?.state === HubConnectionState.Connected) return

    this.state.value = HubConnectionState.Connecting

    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/user', {
        accessTokenFactory: () => this.accessTokenFactory() ?? ''
      })
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(LogLevel.Warning)
      .build()

    this.connection.on('ReceiveEvent', (event: RealtimeEvent) => {
      this.listeners.forEach((l) => l(event))
    })

    this.connection.onreconnecting(() => {
      this.state.value = HubConnectionState.Reconnecting
    })
    this.connection.onreconnected(() => {
      this.state.value = HubConnectionState.Connected
    })
    this.connection.onclose(() => {
      this.state.value = HubConnectionState.Disconnected
    })

    try {
      await this.connection.start()
      this.state.value = HubConnectionState.Connected
    } catch (err) {
      this.state.value = HubConnectionState.Disconnected
      throw err
    }
  }

  async disconnect() {
    await this.connection?.stop()
    this.connection = null
    this.state.value = HubConnectionState.Disconnected
  }

  on(listener: EventListener): () => void {
    this.listeners.add(listener)
    return () => this.listeners.delete(listener)
  }
}

export const realtime = new RealtimeClient()
export { HubConnectionState }
