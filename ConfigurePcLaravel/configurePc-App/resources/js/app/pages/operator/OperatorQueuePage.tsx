import { Package } from 'lucide-react';
import { useProduction } from '../../context/ProductionContext';

export function OperatorQueuePage() {
  const { queue, currentOrder } = useProduction();

  return (
    <div className="space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-white">Production Queue</h1>
        <p className="text-zinc-400 text-sm mt-1">All configured computers waiting to be built</p>
      </div>

      {/* Status badges */}
      <div className="flex items-center gap-3">
        <span className="inline-flex items-center text-xs px-3 py-1 rounded-full border bg-blue-500/20 border-blue-500/30 text-blue-400">
          {queue.length} orders in queue
        </span>
        {currentOrder && (
          <span className="inline-flex items-center text-xs px-3 py-1 rounded-full border bg-green-500/20 border-green-500/30 text-green-400">
            1 order in production
          </span>
        )}
      </div>

      {/* Currently in production */}
      {currentOrder && (
        <div className="rounded-xl p-6 bg-zinc-900 border border-zinc-800">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-green-500/20">
              <Package size={20} className="text-green-400" />
            </div>
            <div>
              <h2 className="text-base font-semibold text-white">Currently in Production</h2>
              <p className="text-sm text-zinc-400">Order being assembled right now</p>
            </div>
          </div>
          <div className="rounded-lg p-4 bg-zinc-800">
            <div className="grid grid-cols-2 gap-4 mb-4">
              <div>
                <p className="text-xs text-zinc-400 mb-1">Order ID</p>
                <p className="text-white font-mono">#{currentOrder.orderId}</p>
              </div>
              <div>
                <p className="text-xs text-zinc-400 mb-1">Created</p>
                <p className="text-white text-sm">{currentOrder.createdAt.toLocaleString()}</p>
              </div>
            </div>
            <div className="border-t border-zinc-700 pt-4">
              <p className="text-sm font-medium text-white mb-3">Queue Payload</p>
              <div className="grid grid-cols-2 gap-3">
                <div className="rounded-lg p-3 bg-zinc-900">
                  <p className="text-xs text-zinc-500 mb-1">Status</p>
                  <p className="text-sm text-white uppercase tracking-wide">{currentOrder.status}</p>
                </div>
                <div className="rounded-lg p-3 bg-zinc-900">
                  <p className="text-xs text-zinc-500 mb-1">Tray Count</p>
                  <p className="text-sm text-white">{currentOrder.itemTrayIds.length}</p>
                </div>
                <div className="rounded-lg p-3 col-span-2 bg-zinc-900">
                  <p className="text-xs text-zinc-500 mb-1">Tray IDs</p>
                  <p className="text-sm text-white">{currentOrder.itemTrayIds.join(', ') || '-'}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Queue list */}
      <div className="space-y-4">
        {queue.map((order, index) => (
          <div
            key={order.orderId}
            className="rounded-xl p-6 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 transition-colors"
          >
            <div className="flex items-start justify-between mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg flex items-center justify-center bg-blue-500/20">
                  <span className="text-blue-400 text-sm font-semibold">#{index + 1}</span>
                </div>
                <div>
                  <p className="text-white font-mono font-semibold">#{order.orderId}</p>
                  <p className="text-sm text-zinc-400">Created: {order.createdAt.toLocaleString()}</p>
                </div>
              </div>
              <span className="inline-flex items-center text-xs px-2 py-0.5 rounded-full border bg-zinc-700/40 border-zinc-700 text-zinc-400">
                {order.status}
              </span>
            </div>
            <div className="grid grid-cols-4 gap-3">
              <div className="rounded-lg p-3 col-span-2 bg-zinc-800">
                <p className="text-xs text-zinc-500 mb-1">Tray IDs</p>
                <p className="text-sm text-white">{order.itemTrayIds.join(', ') || '-'}</p>
              </div>
              <div className="rounded-lg p-3 col-span-2 bg-zinc-800">
                <p className="text-xs text-zinc-500 mb-1">Items</p>
                <p className="text-sm text-white">{order.itemTrayIds.length}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Empty state */}
      {queue.length === 0 && !currentOrder && (
        <div className="rounded-xl p-12 text-center bg-zinc-900 border border-zinc-800">
          <Package size={48} className="text-zinc-600 mx-auto mb-4" />
          <p className="text-zinc-400">No orders in queue</p>
        </div>
      )}
    </div>
  );
}
