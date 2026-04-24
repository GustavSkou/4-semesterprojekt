import { Fragment } from 'react';
import type { ElementType } from 'react';
import {
  Globe, Warehouse, Truck, Wrench, Package,
  CheckCircle, Clock, AlertCircle, Loader2,
  Square, RotateCcw, Play,
} from 'lucide-react';
import { useProduction } from '../../context/ProductionContext';
import type { StageState, ProductionStage } from '../../types/production';

// ─── Stage config ──────────────────────────────────────────────────────────────

const stageIcons: Record<ProductionStage, ElementType> = {
  website:             Globe,
  'warehouse-receive': Warehouse,
  'agv-to-assembly':   Truck,
  assembly:            Wrench,
  'agv-to-warehouse':  Truck,
  'warehouse-delivery':Warehouse,
  delivery:            Package,
};

const stageLabels: Record<ProductionStage, string> = {
  website:              'Website Order',
  'warehouse-receive':  'Warehouse Picking',
  'agv-to-assembly':    'AGV Transport',
  assembly:             'Assembly Station',
  'agv-to-warehouse':   'AGV Return',
  'warehouse-delivery': 'Inserting into Warehouse',
  delivery:             'Out for Delivery',
};

const stages: ProductionStage[] = [
  'website', 'warehouse-receive', 'agv-to-assembly',
  'assembly', 'agv-to-warehouse', 'warehouse-delivery', 'delivery',
];

// ─── Helpers ───────────────────────────────────────────────────────────────────

function stateColor(state: StageState): string {
  switch (state) {
    case 'completed':   return 'bg-green-500/20 border-green-500/30 text-green-400';
    case 'in-progress': return 'bg-blue-500/20 border-blue-500/30 text-blue-400';
    case 'pending':     return 'bg-zinc-700/40 border-zinc-700 text-zinc-400';
    case 'error':       return 'bg-red-500/20 border-red-500/30 text-red-400';
  }
}

function StatusBadge({ state }: { state: StageState }) {
  switch (state) {
    case 'completed':
      return <CheckCircle size={14} className="text-green-500" />;
    case 'in-progress':
      return <Loader2 size={14} className="text-blue-500 animate-spin" />;
    case 'pending':
      return <Clock size={14} className="text-zinc-500" />;
    case 'error':
      return <AlertCircle size={14} className="text-red-500" />;
  }
}

function systemStatusColor(status: string): string {
  if (status === 'running') return 'bg-green-500/20 border-green-500/30 text-green-400';
  if (status === 'stopped') return 'bg-yellow-500/20 border-yellow-500/30 text-yellow-400';
  return 'bg-zinc-700/40 border-zinc-700 text-zinc-400';
}

// ─── Component ─────────────────────────────────────────────────────────────────

export function OperatorDashboardPage() {
  const {
    productionStatus, currentOrder, queue, productionFlow,
    stopProduction, resetProduction, resumeProduction, statusMessage,
  } = useProduction();

  return (
    <div className="space-y-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Production Dashboard</h1>
          <p className="text-zinc-400 text-sm mt-1">Real-time production monitoring and control</p>
        </div>
        <span className={`inline-flex items-center text-xs px-3 py-1 rounded-full border font-medium ${systemStatusColor(productionStatus)}`}>
          {productionStatus.toUpperCase()}
        </span>
      </div>

      {/* Production flow */}
      <div className="rounded-xl p-6 bg-zinc-900 border border-zinc-800">
        <h2 className="text-base font-semibold text-white mb-6">Production Flow</h2>
        <div className="grid gap-2" style={{ gridTemplateColumns: `repeat(${stages.length * 2 - 1}, minmax(0, 1fr))` }}>
          {stages.map((stage, index) => {
            const Icon = stageIcons[stage];
            const state = productionFlow[stage] as StageState;

            return (
              <Fragment key={stage}>
                <div className="flex flex-col items-center gap-2">
                  <div className={`w-14 h-14 rounded-xl border-2 flex items-center justify-center ${stateColor(state)}`}>
                    <Icon size={24} />
                  </div>
                  <div className="text-center">
                    <p className="text-xs font-medium text-white leading-tight">{stageLabels[stage]}</p>
                    <div className="flex items-center justify-center gap-1 mt-1">
                      <StatusBadge state={state} />
                      <span className="text-[10px] text-zinc-500 capitalize">{state}</span>
                    </div>
                  </div>
                </div>
                {index < stages.length - 1 && (
                  <div className="flex items-center justify-center pb-6">
                    <div className="w-full h-0.5 bg-zinc-800" />
                  </div>
                )}
              </Fragment>
            );
          })}
        </div>
      </div>

      {statusMessage && (
        <p className="mt-4 text-sm text-zinc-400 text-center">{statusMessage}</p>
      )}

      {/* Three-column bottom section */}
      <div className="grid grid-cols-3 gap-6">
        {/* System control */}
        <div className="rounded-xl p-6 bg-zinc-900 border border-zinc-800">
          <h2 className="text-base font-semibold text-white mb-4">System Control</h2>
          <div className="space-y-3">
            {productionStatus === 'running' && (
              <button
                onClick={stopProduction}
                className="w-full py-2.5 rounded-lg text-sm font-medium flex items-center justify-center gap-2 bg-red-600 hover:bg-red-700 text-white transition-colors"
              >
                <Square size={15} />
                Stop Production
              </button>
            )}
            {productionStatus === 'stopped' && (
              <>
                <button
                  onClick={resetProduction}
                  className="w-full py-2.5 rounded-lg text-sm font-medium flex items-center justify-center gap-2 bg-yellow-600 hover:bg-yellow-700 text-white transition-colors"
                >
                  <RotateCcw size={15} />
                  Reset Production
                </button>
                <button
                  onClick={resumeProduction}
                  className="w-full py-2.5 rounded-lg text-sm font-medium flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white transition-colors"
                >
                  <Play size={15} />
                  Resume Production
                </button>
              </>
            )}
            {productionStatus === 'reset' && (
              <button
                onClick={resumeProduction}
                className="w-full py-2.5 rounded-lg text-sm font-medium flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700 text-white transition-colors"
              >
                <Play size={15} />
                Resume Production
              </button>
            )}
          </div>
        </div>

        {/* Current order */}
        <div className="rounded-xl p-6 bg-zinc-900 border border-zinc-800">
          <h2 className="text-base font-semibold text-white mb-4">Current Order</h2>
          {currentOrder ? (
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-zinc-400 text-sm">Order ID</span>
                <span className="text-white font-mono text-sm">#{currentOrder.orderId}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-zinc-400 text-sm">Status</span>
                <span className="text-xs text-zinc-200 uppercase tracking-wide">{currentOrder.status}</span>
              </div>
              <div className="border-t border-zinc-800 pt-2 space-y-1.5">
                {[
                  ['Created', currentOrder.createdAt.toLocaleString()],
                  ['Tray count', String(currentOrder.itemTrayIds.length)],
                  ['Tray IDs', currentOrder.itemTrayIds.join(', ') || '-'],
                ].map(([label, value]) => (
                  <div key={label} className="flex justify-between gap-2">
                    <span className="text-xs text-zinc-500">{label}</span>
                    <span className="text-xs text-zinc-300 text-right">{value}</span>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <p className="text-zinc-500 text-sm">No order in production</p>
          )}
        </div>

        {/* Queue preview */}
        <div className="rounded-xl p-6 bg-zinc-900 border border-zinc-800">
          <h2 className="text-base font-semibold text-white mb-4">Production Queue</h2>
          <div className="flex items-center justify-between mb-3">
            <span className="text-zinc-400 text-sm">Orders in queue</span>
            <span className="inline-flex items-center text-xs px-2 py-0.5 rounded-full border bg-blue-500/20 border-blue-500/30 text-blue-400">
              {queue.length}
            </span>
          </div>
          <div className="space-y-2 max-h-36 overflow-y-auto">
            {queue.slice(0, 3).map(order => (
              <div key={order.orderId} className="rounded-lg p-2.5 bg-zinc-800">
                <p className="text-sm text-white font-mono">#{order.orderId}</p>
                <p className="text-xs text-zinc-500 mt-0.5">{order.itemTrayIds.length} tray items</p>
              </div>
            ))}
            {queue.length > 3 && (
              <p className="text-xs text-zinc-500 text-center pt-1">
                +{queue.length - 3} more orders
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
