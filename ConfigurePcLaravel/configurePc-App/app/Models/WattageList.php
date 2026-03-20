<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class WattageList extends Model
{
    protected $fillable = [
        'wattage',
        'component_id',
    ];

    public function component()
    {
        return $this->belongsTo(Component::class);
    }
}
