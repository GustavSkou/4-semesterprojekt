<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Component extends Model
{
    protected $fillable = [
        'name',
        'price',
        'category',
    ];

    public function computers()
    {
        return $this->belongsToMany(Computer::class, 'computer_component_list');
    }

    public function specifications()
    {
        return $this->hasMany(Specification::class);
    }

    public function wattageLists()
    {
        return $this->hasMany(WattageList::class);
    }

    public function requirements()
    {
        return $this->hasMany(Requirement::class);
    }
}
